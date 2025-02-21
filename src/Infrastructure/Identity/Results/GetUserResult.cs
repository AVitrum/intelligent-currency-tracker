using Domain.Common;
using Shared.Dtos;

namespace Infrastructure.Identity.Results;

public class GetUserResult : BaseResult
{
    private GetUserResult(bool success, IEnumerable<string> errors, UserDto data) : base(success, errors)
    {
        Data = data;
    }

    public UserDto Data { get; private set; }

    public static GetUserResult SuccessResult(UserDto user)
    {
        return new GetUserResult(true, [], user);
    }
}