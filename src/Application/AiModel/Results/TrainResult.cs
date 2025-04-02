using Domain.Common;

namespace Application.AiModel.Results;

public class TrainResult : BaseResult
{
    private TrainResult(bool success, IEnumerable<string> errors, string message) : base(success, errors)
    {
        Message = message;
    }

    public string Message { get; }

    public static TrainResult SuccessResult(string message)
    {
        return new TrainResult(true, Array.Empty<string>(), message);
    }
}