using Application.Common.Interfaces.Services;
using Domain.Enums;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MinioController : ControllerBase
{
    private readonly IMinioService _minioService;

    public MinioController(IMinioService minioService)
    {
        _minioService = minioService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadAsync(IFormFile? file, [FromForm] Dictionary<string, string>? tags)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("File is empty");
        }

        var filePath = Path.GetTempFileName();
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        Dictionary<FileTag, string>? enumTags = null;

        if (tags is not null && tags.Count != 0)
        {
            enumTags = new Dictionary<FileTag, string>();

            foreach (var tag in tags)
                if (Enum.TryParse<FileTag>(tag.Key, true, out var fileTag))
                {
                    enumTags[fileTag] = tag.Value;
                }
        }

        var key = await _minioService.UploadFileAsync(filePath, file.FileName, enumTags);

        return Ok(new { Message = "File uploaded successfully", FileName = key });
    }

    [HttpGet("download/{fileName}")]
    public async Task<IActionResult> DownloadAsync(string fileName)
    {
        var fileBytes = await _minioService.DownloadFileAsync(fileName);

        if (fileBytes.Length == 0)
        {
            return NotFound("File not found");
        }

        return File(fileBytes, "application/octet-stream", fileName);
    }

    [HttpGet("tags/{fileName}")]
    public async Task<IActionResult> GetTagsAsync(string fileName)
    {
        var tags = await _minioService.GetTagsAsync(fileName);

        return Ok(tags);
    }
}