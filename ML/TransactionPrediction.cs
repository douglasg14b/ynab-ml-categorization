namespace YnabCategoryAi.ML;

public class TransactionPrediction
{
    // For classification tasks, you usually have a PredictedLabel and a Probability
    // The PredictedLabel property holds the predicted category or class
    public string PredictedLabel { get; set; }

    // If your model outputs confidence scores or probabilities, include them as well
    // This could be an array of scores or a single probability score, depending on your model
    public float[] Score { get; set; }
}