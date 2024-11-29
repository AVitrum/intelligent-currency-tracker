using Domain.Common;

namespace Infrastructure.Identity;

public class IdentityServiceResult : BaseResult
{
    public string Token { get; init; }
    
    private IdentityServiceResult(bool success, IEnumerable<string> errors, string token) : base(success, errors)
    {
        Token = token;
    }
    
    public static IdentityServiceResult SuccessResult() => new(true, [], string.Empty);
    public static IdentityServiceResult FailureResult(IEnumerable<string> errors) => new(false, errors, string.Empty);
    public static IdentityServiceResult ReturnTokenResult(string token) => new(true, [], token);
}