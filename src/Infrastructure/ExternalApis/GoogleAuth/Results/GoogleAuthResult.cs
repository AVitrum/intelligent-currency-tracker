using Domain.Common;

namespace Infrastructure.ExternalApis.GoogleAuth.Results;

public class GoogleAuthResult : BaseResult
{
    private GoogleAuthResult(bool success, IEnumerable<string> errors, string token) : base(success, errors)
    {
        Token = token;
    }

    public string Token { get; }

    public static GoogleAuthResult SuccessResult(string token)
    {
        return new GoogleAuthResult(true, Array.Empty<string>(), token);
    }

    public new static GoogleAuthResult FailureResult(IEnumerable<string> errors)
    {
        return new GoogleAuthResult(false, errors, string.Empty);
    }
}