using Domain.Common;

namespace Shared.Payload.Responses.AiModel;

public class PredictResponse : BaseResponse
{
    public PredictResponse(
        bool success,
        string message,
        int statusCode,
        IEnumerable<string> errors,
        PredictionResponse? prediction) : base(success, message, statusCode, errors)
    {
        Prediction = prediction;
    }

    public PredictionResponse? Prediction { get; }
}