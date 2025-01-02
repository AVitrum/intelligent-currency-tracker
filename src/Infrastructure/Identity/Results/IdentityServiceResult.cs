using Domain.Common;

namespace Infrastructure.Identity.Results;

public class IdentityServiceResult : BaseResult
{
    public string Token { get; }
    
    private IdentityServiceResult(bool success, IEnumerable<string> errors, string token) : base(success, errors)
    {
        Token = token;
    }
    
    public static IdentityServiceResult ReturnTokenResult(string token) => new(true, [], token);
}