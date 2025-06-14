using Domain.Common;
using Shared.Dtos;

namespace Shared.Payload.Responses.Post;

public class GetPostByIdResponse : BaseResponse
{
    public PostDto Post { get; set; }
    
    public GetPostByIdResponse(bool success, string message, int statusCode, IEnumerable<string> errors, PostDto post) : base(success, message, statusCode, errors)
    {
        Post = post;
    }
}