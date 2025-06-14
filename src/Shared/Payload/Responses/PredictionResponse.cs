using System.Text.Json.Serialization;

public class Prediction
{
    [JsonPropertyName("ds")]
    public string Ds { get; set; }

    [JsonPropertyName("yhat")]
    public double Yhat { get; set; }

    public DateTime Date => DateTime.Parse(Ds);
}

public class PredictionResponse
{
    [JsonPropertyName("prediction")]
    public List<Prediction> Predictions { get; set; } = null!;
}

public class ModelForecastResponse
{
    [JsonPropertyName("forecast")]
    public List<Prediction> Forecast { get; set; } = null!;
}