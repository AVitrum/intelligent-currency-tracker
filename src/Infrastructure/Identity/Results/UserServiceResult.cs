using Domain.Common;

namespace Infrastructure.Identity.Results;

public class UserServiceResult : BaseResult
{
    private UserServiceResult(bool success, IEnumerable<string> errors, string token, string refreshToken) : base(
        success, errors)
    {
        Token = token;
        RefreshToken = refreshToken;
    }

    public string Token { get; }
    public string RefreshToken { get; }

    public static UserServiceResult ReturnTokenResult(string token, string refreshToken)
    {
        return new UserServiceResult(true, [], token, refreshToken);
    }
}