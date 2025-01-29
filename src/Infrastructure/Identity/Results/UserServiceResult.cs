using Domain.Common;

namespace Infrastructure.Identity.Results;

public class UserServiceResult : BaseResult
{
    private UserServiceResult(bool success, IEnumerable<string> errors, string token) : base(success, errors)
    {
        Token = token;
    }

    public string Token { get; }

    public static UserServiceResult ReturnTokenResult(string token)
    {
        return new UserServiceResult(true, [], token);
    }
}