using System.Text.Json.Serialization;

public class Prediction
{
    [JsonPropertyName("ds")]
    public DateTime Ds { get; set; }

    [JsonPropertyName("yhat")]
    public double Yhat { get; set; }

    [JsonPropertyName("yhat_lower")]
    public double YhatLower { get; set; }

    [JsonPropertyName("yhat_upper")]
    public double YhatUpper { get; set; }
}

public class PredictionResponse
{
    [JsonPropertyName("prediction")]
    public List<Prediction> Predictions { get; set; } = null!;
}