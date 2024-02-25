using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.LightGbm;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Text;

namespace YnabCategoryAi.ML;

public static class TransactionsML
{
    private const string ModelPath = "model.zip";
    private static PredictionEngine<TransactionPayeeDto, TransactionPrediction>? _predictionEngine;
    public static MLContext MlContext { get; } = new(0);

    public static IDataView Data => MlContext.Data.LoadFromEnumerable(Utilities.GetExpandedTransactionsData().Select(x =>
        new TransactionPayeeDto
        {
            PayeeName = x.PayeeName.Preprocess(),
            ImportPayeeNameOriginal = x.ImportPayeeNameOriginal.Preprocess()
        }).ToList());


    public static IDataView TestData => MlContext.Data.LoadFromEnumerable(Utilities.GetUnapprovedTransactions().Select(x =>
        new TransactionPayeeDto
        {
            PayeeName = x.PayeeName.Preprocess(),
            ImportPayeeNameOriginal = x.ImportPayeeNameOriginal.Preprocess()
        }).ToList());

    public static PredictionEngine<TransactionPayeeDto, TransactionPrediction> GetPredictionEngine()
    {
        if (_predictionEngine == null)
        {
            ITransformer model = GetOrTrainModel();
            _predictionEngine = MlContext.Model.CreatePredictionEngine<TransactionPayeeDto, TransactionPrediction>(model);
        }

        return _predictionEngine;
    }

    public static EstimatorChain<KeyToValueMappingTransformer> GetTrainingPipeline()
    {
        EstimatorChain<ColumnConcatenatingTransformer>? dataProcessPipeline = MlContext.Transforms.Conversion
            .MapValueToKey(nameof(TransactionPayeeDto.PayeeName))
            .Append(MlContext.Transforms.Text.FeaturizeText("Features",
                new TextFeaturizingEstimator.Options
                {
                    WordFeatureExtractor = new WordBagEstimator.Options { NgramLength = 2, UseAllLengths = true },
                    CharFeatureExtractor = new WordBagEstimator.Options { NgramLength = 3, UseAllLengths = true },
                    Norm = TextFeaturizingEstimator.NormFunction.L2
                },
                nameof(TransactionPayeeDto.ImportPayeeNameOriginal)))
            .Append(MlContext.Transforms.Concatenate("Features", "Features"))
            .AppendCacheCheckpoint(MlContext);


        LightGbmMulticlassTrainer? trainer =
            MlContext.MulticlassClassification.Trainers.LightGbm(nameof(TransactionPayeeDto.PayeeName));

        return dataProcessPipeline.Append(trainer)
            .Append(MlContext.Transforms.Conversion.MapKeyToValue(nameof(TransactionPrediction.PredictedLabel),
                nameof(TransactionPrediction.PredictedLabel)));
    }

    public static ITransformer GetOrTrainModel()
    {
        if (!File.Exists(ModelPath))
        {
            TrainModel();
        }

        return MlContext.Model.Load("model.zip", out _);
    }

    public static void RetrainModel()
    {
        if (File.Exists(ModelPath))
        {
            File.Delete(ModelPath);
        }

        TrainModel();
    }

    public static void TrainModel()
    {
        EstimatorChain<KeyToValueMappingTransformer> trainingPipeline = GetTrainingPipeline();

        TransformerChain<KeyToValueMappingTransformer>? trainedModel = trainingPipeline.Fit(Data);

        MlContext.Model.Save(trainedModel, Data.Schema, ModelPath);
    }

    public static PredictionResultWithConfidence Predict(TransactionPayeeDto transaction, float confidenceThreshold)
    {
        TransactionPrediction? prediction = GetPredictionEngine().Predict(transaction);

        // Assuming the score array is ordered the same as the model's classes and that higher score means higher confidence
        float maxScore = prediction.Score.Max();
        int maxIndex = Array.IndexOf(prediction.Score, maxScore);
        string predictedLabel = prediction.PredictedLabel;

        // Check if the highest confidence score is above the threshold
        bool isAboveThreshold = maxScore >= confidenceThreshold;

        return new PredictionResultWithConfidence
        {
            Input = transaction.ImportPayeeNameOriginal,
            PredictedLabel = predictedLabel,
            ConfidenceScore = maxScore,
            ConfidenceScores = prediction.Score,
            CosineSimilarity = transaction.ImportPayeeNameOriginal.GetSimilarity(predictedLabel),
            IsAboveConfidenceThreshold = isAboveThreshold
        };
    }

    public static CrossValidationResultDto CrossValidateModel()
    {
        IReadOnlyList<TrainCatalogBase.CrossValidationResult<MulticlassClassificationMetrics>> cvResults =
            MlContext.MulticlassClassification.CrossValidate(TestData,
                GetTrainingPipeline(),
                labelColumnName: nameof(TransactionPayeeDto.PayeeName));

        double averageMacroAccuracy = cvResults.Select(r => r.Metrics.MacroAccuracy).Average();
        double averageLogLoss = cvResults.Select(r => r.Metrics.LogLoss).Average();

        return new CrossValidationResultDto(averageMacroAccuracy, averageLogLoss, cvResults);
    }

    public static MulticlassClassificationMetrics EvaluateModel()
    {
        ITransformer model = GetOrTrainModel();
        IDataView? predictions = model.Transform(TestData);

        return MlContext.MulticlassClassification.Evaluate(predictions, nameof(TransactionPayeeDto.PayeeName));
    }
}