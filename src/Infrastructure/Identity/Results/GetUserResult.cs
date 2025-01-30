using Domain.Common;
using Shared.Dtos;

namespace Infrastructure.Identity.Results;

public class GetUserResult : BaseResult
{
    private GetUserResult(bool success, IEnumerable<string> errors, IEnumerable<UserDto> data) : base(success, errors)
    {
        Data = data;
    }

    public IEnumerable<UserDto> Data { get; }

    public static GetUserResult SuccessResult(IEnumerable<UserDto> users)
    {
        return new GetUserResult(true, [], users);
    }
}