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

        return Ok(new { Message = "File uploaded successfully", FileName = key });
    }

    [HttpGet("download/{fileName}")]
    public async Task<IActionResult> DownloadAsync(string fileName)
    {
        byte[]? fileBytes = await _minioService.DownloadFileAsync(fileName);

        if (fileBytes.Length == 0)
        {
            return NotFound("File not found");
        }

        return File(fileBytes, "application/octet-stream", fileName);
    }

    [HttpGet("tags/{fileName}")]
    public async Task<IActionResult> GetTagsAsync(string fileName)
    {
        Dictionary<FileTag, string>? tags = await _minioService.GetTagsAsync(fileName);

        return Ok(tags);
    }
}