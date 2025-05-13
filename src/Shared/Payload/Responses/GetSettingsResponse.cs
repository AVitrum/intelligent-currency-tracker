using Domain.Common;
using Shared.Dtos;

namespace Shared.Payload.Responses;

public class GetSettingsResponse : BaseResponse
{
    public GetSettingsResponse(
        bool success,
        string message,
        int statusCode,
        IEnumerable<string> errors,
        SettingsDto? settings) : base(success, message, statusCode, errors)
    {
        Settings = settings;
    }

    public SettingsDto? Settings { get; set; }
}