using Domain.Common;
using Shared.Dtos;

namespace Application.Posts.Results;

public class GetPostByIdResult : BaseResult
{
    public PostDto Post { get; set; }

    private GetPostByIdResult(bool success, IEnumerable<string> errors, PostDto post) : base(success, errors)
    {
        Post = post;
    }

    public static GetPostByIdResult SuccessResult(PostDto post)
    {
        return new GetPostByIdResult(true, [], post);
    }
}