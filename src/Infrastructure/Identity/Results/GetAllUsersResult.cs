using Domain.Common;
using Shared.Dtos;

namespace Infrastructure.Identity.Results;

public class GetAllUsersResult : BaseResult
{
    private GetAllUsersResult(bool success, IEnumerable<string> errors, IEnumerable<UserDto> data) : base(success,
        errors)
    {
        Data = data;
    }

    public IEnumerable<UserDto> Data { get; }

    public static GetAllUsersResult SuccessResult(IEnumerable<UserDto> users)
    {
        return new GetAllUsersResult(true, [], users);
    }
}