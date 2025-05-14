using Domain.Common;

namespace Shared.Payload.Responses;

public class SaveSettingsResponse(bool success, string message, int statusCode, IEnumerable<string> errors)
    : BaseResponse(success, message, statusCode, errors);