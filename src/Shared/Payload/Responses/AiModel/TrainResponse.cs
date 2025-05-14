using Domain.Common;

namespace Shared.Payload.Responses.AiModel;

public class TrainResponse : BaseResponse
{
    public TrainResponse(bool success, string message, int statusCode, IEnumerable<string> errors) : base(success,
        message, statusCode, errors) { }
}