namespace YnabCategoryAi.ML;

public class PredictionResultWithConfidence
{
    public string Input { get; set; }
    public string PredictedLabel { get; set; }
    public float ConfidenceScore { get; set; }
    public float[] ConfidenceScores { get; set; }

    public double CosineSimilarity { get; set; }
    public bool IsAboveConfidenceThreshold { get; set; }
}