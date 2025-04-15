using Microsoft.AspNetCore.Http;

namespace Application.Common.Payload.Dtos;

public class CsvFileUploadDto
{
    public IFormFile? File { get; set; }
}