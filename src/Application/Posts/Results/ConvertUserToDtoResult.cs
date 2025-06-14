using Domain.Common;
using Shared.Dtos;

namespace Application.Posts.Results;

public class ConvertUserToDtoResult : BaseResult
{
    public UserDto User { get; set; }

    private ConvertUserToDtoResult(bool success, IEnumerable<string> errors, UserDto user) : base(success, errors)
    {
        User = user;
    }

    public static ConvertUserToDtoResult SuccessResult(UserDto user)
    {
        return new ConvertUserToDtoResult(true, [], user);
    }
}