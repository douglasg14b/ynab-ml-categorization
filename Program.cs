// See https://aka.ms/new-console-template for more information

using Microsoft.ML;
using Microsoft.ML.Data;
using YnabCategoryAi;
using YnabCategoryAi.ML;

string[] testNames = Utilities.GetUnapprovedTransactions().Select(x => x.ImportPayeeNameOriginal).ToArray();

PredictionResultWithConfidence[] predictions = testNames
    .Select(x => TransactionsML.Predict(new TransactionPayeeDto { ImportPayeeNameOriginal = x.Preprocess() }, 0.90f)).ToArray();

PredictionResultWithConfidence[] aboveThreshold = predictions.Where(x => x.IsAboveConfidenceThreshold).ToArray();
PredictionResultWithConfidence[] belowThreshold = predictions.Where(x => !x.IsAboveConfidenceThreshold).ToArray();

PredictionResultWithConfidence[] belowCosignSimilarity = aboveThreshold.Where(x => x.CosineSimilarity < 0.5).ToArray();
PredictionResultWithConfidence[] aboveCosignSimilarity = aboveThreshold.Where(x => x.CosineSimilarity >= 0.5).ToArray();

CrossValidationResultDto result = TransactionsML.CrossValidateModel();

List<PredictionInspection> poorPredictions = ExtractPoorPredictions(TransactionsML.TestData, result.DetailedResults);

Console.WriteLine($"Average MacroAccuracy: {result.AverageMacroAccuracy}");
Console.WriteLine($"Average LogLoss: {result.AverageLogLoss}");

return;

static List<PredictionInspection> ExtractPoorPredictions(IDataView testData,
    IReadOnlyList<TrainCatalogBase.CrossValidationResult<MulticlassClassificationMetrics>> cvResults)
{
    var poorPredictions = new List<PredictionInspection>();
    foreach (TrainCatalogBase.CrossValidationResult<MulticlassClassificationMetrics> fold in cvResults)
    {
        // Get the model from the fold
        ITransformer? model = fold.Model;

        // Apply the model to the testData to get predictions
        IDataView predictions = model.Transform(testData);

        // Convert IDataView to an IEnumerable of your prediction objects
        IEnumerable<TransactionPrediction>? predictionObjects =
            TransactionsML.MlContext.Data.CreateEnumerable<TransactionPrediction>(predictions, false);

        // Assuming you have access to the original data with correct labels
        List<TransactionPayeeDto> originalData =
            TransactionsML.MlContext.Data.CreateEnumerable<TransactionPayeeDto>(testData, false).ToList();

        // Zip original data with predictions to evaluate each prediction
        var zippedResults = originalData.Zip(predictionObjects,
            (original, prediction) => new { Original = original, Prediction = prediction });

        foreach (var result in zippedResults)
        {
            if (result.Original.PayeeName != result.Prediction.PredictedLabel) // Criterion for poor prediction
            {
                poorPredictions.Add(new PredictionInspection
                {
                    CorrectLabel = result.Original.PayeeName,
                    PredictedLabel = result.Prediction.PredictedLabel,
                    ImportPayeeNameOriginal = result.Original.ImportPayeeNameOriginal,
                    Confidence = result.Prediction.Score.Max() // Assuming you define confidence as the max score
                });
            }
        }
    }

    return poorPredictions;
}

public class PredictionInspection
{
    public string CorrectLabel { get; set; }
    public string PredictedLabel { get; set; }
    public string ImportPayeeNameOriginal { get; set; }
    public float Confidence { get; set; }
}