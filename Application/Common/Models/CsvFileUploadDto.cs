using Microsoft.AspNetCore.Http;

namespace Application.Common.Models;

public class CsvFileUploadDto
{
    public IFormFile? File { get; set; }
}