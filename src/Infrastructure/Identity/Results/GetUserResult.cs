using Domain.Common;
using Shared.Payload;

namespace Infrastructure.Identity.Results;

public class GetUserResult : BaseResult
{
    public IEnumerable<UserDto> Data { get; }

    private GetUserResult(bool success, IEnumerable<string> errors, IEnumerable<UserDto> data) : base(success, errors)
    {
        Data = data;
    }

    public static GetUserResult SuccessResult(IEnumerable<UserDto> users)
    {
        return new GetUserResult(true, [], users);
    }
}