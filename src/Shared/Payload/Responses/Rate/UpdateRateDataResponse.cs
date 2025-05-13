using Domain.Common;

namespace Shared.Payload.Responses.Rate;

public class UpdateRateDataResponse(bool success, string message, int statusCode, IEnumerable<string> errors)
    : BaseResponse(success, message, statusCode, errors);