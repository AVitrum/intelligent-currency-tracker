using Domain.Common;

namespace Infrastructure.Identity.Results;

public class GetUserRoleResult : BaseResult
{
    public string Role { get; private set; }

    private GetUserRoleResult(bool success, IEnumerable<string> errors, string role) : base(success, errors)
    {
        Role = role;
    }
    
    public static GetUserRoleResult SuccessResult(string role)
    {
        return new GetUserRoleResult(true, [], role);
    }
}