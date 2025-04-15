using Domain.Common;

namespace Infrastructure.GoogleAuth.Results;

public class GoogleAuthResult : BaseResult
{
    private GoogleAuthResult(bool success, IEnumerable<string> errors, string token, string refreshToken) : base(
        success, errors)
    {
        Token = token;
        RefreshToken = refreshToken;
    }

    public string Token { get; }
    public string RefreshToken { get; }

    public static GoogleAuthResult SuccessResult(string token, string refreshToken)
    {
        return new GoogleAuthResult(true, Array.Empty<string>(), token, refreshToken);
    }

    public new static GoogleAuthResult FailureResult(IEnumerable<string> errors)
    {
        return new GoogleAuthResult(false, errors, string.Empty, string.Empty);
    }
}