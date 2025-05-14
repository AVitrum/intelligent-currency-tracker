using Domain.Common;

namespace Shared.Payload.Responses.Identity;

public class LoginResponse : BaseResponse
{
    public LoginResponse(
        bool success,
        string message,
        int statusCode,
        IEnumerable<string> errors,
        string token,
        string refreshToken) : base(success, message, statusCode, errors)
    {
        Token = token;
        RefreshToken = refreshToken;
    }

    public string Token { get; }
    public string RefreshToken { get; }
}