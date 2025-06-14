using Domain.Common;
using Shared.Dtos;

namespace Shared.Payload.Responses.Post;

public class GetAllPostsResponse : BaseResponse
{
    public GetAllPostsResponse(bool success, string message, int statusCode, IEnumerable<string> errors, IEnumerable<PostDto> posts) : base(success,
        message, statusCode, errors)
    {
        Posts = posts;
    }
    
    public IEnumerable<PostDto> Posts { get; set; }
}