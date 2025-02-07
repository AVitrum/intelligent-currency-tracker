using Domain.Common;
using Shared.Dtos;

namespace Infrastructure.Identity.Results;

public class GetUserResult : BaseResult
{
    public UserDto Data { get; private set; }
    
    private GetUserResult(bool success, IEnumerable<string> errors, UserDto data) : base(success, errors)
    {
        Data = data;
    }
    
    public static GetUserResult SuccessResult(UserDto user) => new(true, [], user);
}