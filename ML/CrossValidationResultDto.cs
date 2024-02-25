using Microsoft.ML;
using Microsoft.ML.Data;

namespace YnabCategoryAi.ML;

public class CrossValidationResultDto
{
    /// <summary>
    ///     MacroAccuracy: This metric calculates the average accuracy across all classes, giving equal weight to each class
    ///     regardless of its frequency in the dataset. It's particularly useful in datasets with imbalanced classes. A higher
    ///     MacroAccuracy (closer to 1.0) indicates better overall performance across all classes, suggesting that your model
    ///     performs well even on less frequent classes.
    /// </summary>
    public double AverageMacroAccuracy { get; set; }


    /// <summary>
    ///     LogLoss: Logarithmic Loss, or LogLoss, measures the accuracy of a classifier by penalizing false classifications.
    ///     Lower LogLoss values indicate better model performance, with 0 representing a perfect model. LogLoss is sensitive
    ///     to the confidence of the predictions, meaning that being very wrong with high confidence will result in a higher
    ///     penalty.
    /// </summary>
    public double AverageLogLoss { get; set; }

    public IReadOnlyList<TrainCatalogBase.CrossValidationResult<MulticlassClassificationMetrics>> DetailedResults { get; set; }

    public CrossValidationResultDto(double averageMacroAccuracy,
        double averageLogLoss,
        IReadOnlyList<TrainCatalogBase.CrossValidationResult<MulticlassClassificationMetrics>> detailedResults)
    {
        AverageMacroAccuracy = averageMacroAccuracy;
        AverageLogLoss = averageLogLoss;
        DetailedResults = detailedResults;
    }
}