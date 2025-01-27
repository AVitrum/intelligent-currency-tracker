using Domain.Common;

namespace Infrastructure.Identity.Results;

public class IdentityServiceResult : BaseResult
{
    private IdentityServiceResult(bool success, IEnumerable<string> errors, string token) : base(success, errors)
    {
        Token = token;
    }

    public string Token { get; }

    public static IdentityServiceResult ReturnTokenResult(string token)
    {
        return new IdentityServiceResult(true, [], token);
    }
}