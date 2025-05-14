using Domain.Common;

namespace Application.AiModel.Results;

public class PredictResult : BaseResult
{
    private PredictResult(bool success, IEnumerable<string> errors, PredictionResponse? prediction) : base(success,
        errors)
    {
        Prediction = prediction;
    }

    public PredictionResponse? Prediction { get; }

    public static PredictResult SuccessResult(PredictionResponse? prediction)
    {
        return new PredictResult(true, [], prediction);
    }
}