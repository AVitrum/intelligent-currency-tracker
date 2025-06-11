using Domain.Common;
using Shared.Dtos;

namespace Application.Posts.Results;

public class GetAllPostsResult : BaseResult
{
    public IEnumerable<PostDto> Posts { get; set; }

    private GetAllPostsResult(bool success, IEnumerable<string> errors, IEnumerable<PostDto> posts) : base(success,
        errors)
    {
        Posts = posts;
    }

    public static GetAllPostsResult SuccessResult(IEnumerable<PostDto> posts)
    {
        return new GetAllPostsResult(true, [], posts);
    }
}