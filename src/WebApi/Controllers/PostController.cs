using Application.Common.Interfaces.Services;
using Application.Posts.Results;
using Domain.Common;
using Domain.Enums;
using Infrastructure.Utils.Extensions;
using Microsoft.AspNetCore.Authorization;
using Shared.Payload.Requests;
using Shared.Payload.Responses;
using Shared.Payload.Responses.Post;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PostController(IPostService postService, IHttpContextAccessor httpContextAccessor)
    {
        _postService = postService;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpPost("create")]
    [Authorize(Roles = nameof(UserRole.PUBLISHER))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BaseResult))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(BaseResult))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(BaseResult))]
    public async Task<ActionResult<BaseResult>> CreatePost(
        [FromForm] string title,
        [FromForm] string content,
        [FromForm] string category,
        [FromForm] Language? language,
        [FromForm] IFormFileCollection? attachments)
    {
        if (ValidateString(title, out DefaultResponse? defResponse) ||
            ValidateString(content, out defResponse) ||
            ValidateString(category, out defResponse))
        {
            return BadRequest(defResponse);
        }

        string userId = _httpContextAccessor.HttpContext?.User.GetUserId()!;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        CreatePostRequest request = new CreatePostRequest
        {
            Title = title,
            Content = content,
            Category = category,
            Language = language?.ToString() ?? nameof(Language.En),
            AuthorId = userId
        };
        if (attachments is not null && attachments.Any())
        {
            foreach (IFormFile file in attachments)
                if (file.Length > 0)
                {
                    string filePath = Path.GetTempFileName();
                    await using (FileStream stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    string fileExtension = Path.GetExtension(file.FileName);

                    request.Attachments ??= new List<(string, string)>();
                    request.Attachments.Add((filePath, fileExtension));
                }
        }

        BaseResult result = await _postService.CreateAsync(request);

        if (result.Success)
        {
            return Ok(new DefaultResponse(
                true,
                "Report sent successfully.",
                StatusCodes.Status200OK,
                []));
        }

        return StatusCode(StatusCodes.Status500InternalServerError, new DefaultResponse(
            false,
            "Failed to send report.",
            StatusCodes.Status500InternalServerError,
            result.Errors));
    }

    [HttpGet("get-all")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetAllPostsResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(BaseResult))]
    public async Task<ActionResult<BaseResult>> GetAllPosts(
        [FromQuery] string language = nameof(Language.En),
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1)
        {
            return BadRequest(new DefaultResponse(
                false,
                "Page and page size must be greater than zero.",
                StatusCodes.Status400BadRequest,
                new List<string> { "Invalid pagination parameters." }));
        }

        BaseResult result = await _postService.GetAllAsync(language, page, pageSize);

        if (result is not GetAllPostsResult getAllPostsResult)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new DefaultResponse(
                false,
                "Failed to retrieve posts.",
                StatusCodes.Status500InternalServerError,
                result.Errors));
        }

        if (getAllPostsResult.Posts.ToList().Count == 0)
        {
            return NotFound(new DefaultResponse(
                false,
                "No posts found.",
                StatusCodes.Status404NotFound,
                ["Posts not found."]
            ));
        }

        return Ok(new GetAllPostsResponse(
            true,
            "Posts retrieved successfully.",
            StatusCodes.Status200OK,
            [],
            getAllPostsResult.Posts));
    }

    [HttpGet("get-by-id/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(BaseResult))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(BaseResult))]
    public async Task<ActionResult<BaseResult>> GetPostById(Guid id)
    {
        if (id == Guid.Empty)
        {
            return NotFound(new DefaultResponse(
                false,
                "Invalid post ID.",
                StatusCodes.Status400BadRequest,
                new List<string> { "Post ID cannot be empty." }));
        }

        BaseResult result = await _postService.GetById(id);

        if (result is not GetPostByIdResult getPostByIdResult)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new DefaultResponse(
                false,
                "Failed to retrieve post.",
                StatusCodes.Status500InternalServerError,
                result.Errors));
        }

        return Ok(new GetPostByIdResponse(
            true,
            "Post retrieved successfully.",
            StatusCodes.Status200OK,
            [],
            getPostByIdResult.Post));
    }

    [HttpGet("get-attachments-by-id/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(BaseResult))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(BaseResult))]
    public async Task<ActionResult<BaseResult>> GetAttachmentsById(Guid id)
    {
        if (id == Guid.Empty)
        {
            return NotFound(new DefaultResponse(
                false,
                "Invalid post ID.",
                StatusCodes.Status400BadRequest,
                new List<string> { "Post ID cannot be empty." }));
        }

        BaseResult result = await _postService.GetAttachmentsById(id);
        if (result is not GetAttachmentsByIdResult getAttachmentsByIdResult)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new DefaultResponse(
                false,
                "Failed to process request.",
                StatusCodes.Status500InternalServerError,
                result.Errors));
        }

        if (getAttachmentsByIdResult.Attachments.ToList().Count == 0)
        {
            return NotFound(new DefaultResponse(
                false,
                "No attachments found for the specified post.",
                StatusCodes.Status404NotFound,
                new List<string> { "No attachments found." }));
        }

        return Ok(new GetAttachmentsByIdResponse(
            true,
            "Attachments retrieved successfully.",
            StatusCodes.Status200OK,
            [],
            getAttachmentsByIdResult.Attachments));
    }

    private static bool ValidateString(string s, out DefaultResponse? response)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            response = new DefaultResponse(
                false,
                "Validation failed.",
                StatusCodes.Status400BadRequest,
                new List<string> { "Validation failed." });

            return true;
        }

        response = null;
        return false;
    }
}