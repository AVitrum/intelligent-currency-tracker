using Domain.Common;

namespace Shared.Payload.Responses.AiModel;

public class ForecastResponse : BaseResponse
{
    public ForecastResponse(
        bool success,
        string message,
        int statusCode,
        IEnumerable<string> errors,
        List<Prediction>? forecast) : base(success, message, statusCode, errors)
    {
        Forecast = forecast;
    }

    public List<Prediction>? Forecast { get; set; }
}