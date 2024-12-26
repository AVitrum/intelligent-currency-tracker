using Domain.Common;

namespace Infrastructure.ExternalApis.GoogleAuth.Results;

public class GoogleAuthResult : BaseResult
{
    public bool NeedsPassword { get; set; }
    public string Token { get; set; }

    private GoogleAuthResult(bool success, IEnumerable<string> errors, bool needsPassword, string token) : base(success, errors)
    {
        NeedsPassword = needsPassword;
        Token = token;
    }
    
    public static GoogleAuthResult SuccessResult(string token) => new(true, Array.Empty<string>(), false, token);
    public static GoogleAuthResult FailureResult(string error) => new(false, [error], false, string.Empty);
    public static GoogleAuthResult NeedPassword() => new(false, Array.Empty<string>(), true, string.Empty);
}