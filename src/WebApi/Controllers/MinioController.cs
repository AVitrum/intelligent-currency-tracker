using Application.Common.Interfaces.Services;
using Domain.Common;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Shared.Payload.Responses.Minio;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MinioController : ControllerBase
{
    private readonly IMinioService _minioService;

    public MinioController(IMinioService minioService)
    {
        _minioService = minioService;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<BaseResponse>> UploadAsync(
        IFormFile? file,
        [FromForm] Dictionary<string, string>? tags)
    {
        BaseResponse response;

        if (file is null || file.Length == 0)
        {
            response = new UploadResponse(
                false,
                "File is empty",
                StatusCodes.Status400BadRequest,
                new List<string> { "File is empty" });

            return BadRequest(response);
        }

        string filePath = Path.GetTempFileName();
        await using (FileStream stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        Dictionary<FileTag, string>? enumTags = null;

        if (tags is not null && tags.Count != 0)
        {
            enumTags = new Dictionary<FileTag, string>();

            foreach (KeyValuePair<string, string> tag in tags)
                if (Enum.TryParse(tag.Key, true, out FileTag fileTag))
                {
                    enumTags[fileTag] = tag.Value;
                }
        }

        string key = await _minioService.UploadFileAsync(filePath, file.FileName, enumTags);

        response = new UploadResponse(
            true,
            "File uploaded successfully",
            StatusCodes.Status200OK,
            new List<string>());

        return Ok(response);
    }

    [HttpGet("download/{fileName}")]
    public async Task<IActionResult> DownloadAsync(string fileName)
    {
        byte[] fileBytes = await _minioService.DownloadFileAsync(fileName);

        if (fileBytes.Length == 0)
        {
            return NotFound(new DownloadResponse(
                false,
                "File not found",
                StatusCodes.Status404NotFound,
                new List<string> { "File not found" }));
        }

        return File(fileBytes, "application/octet-stream", fileName);
    }
}