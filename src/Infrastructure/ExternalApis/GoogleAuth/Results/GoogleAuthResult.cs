using Domain.Common;

namespace Infrastructure.ExternalApis.GoogleAuth.Results;

public class GoogleAuthResult : BaseResult
{
    public string Token { get; }

    private GoogleAuthResult(bool success, IEnumerable<string> errors, string token) : base(success, errors)
    {
        Token = token;
    }
    
    public static GoogleAuthResult SuccessResult(string token) => new(true, Array.Empty<string>(), token);
    public new static GoogleAuthResult FailureResult(IEnumerable<string> errors) => new(false, errors, string.Empty);
}