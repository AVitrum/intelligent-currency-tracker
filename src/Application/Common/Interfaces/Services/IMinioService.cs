using Domain.Enums;

namespace Application.Common.Interfaces.Services;

public interface IMinioService
{
    Task<string> UploadFileAsync(string filePath, string key, Dictionary<FileTag, string>? tags = null);
    Task<byte[]> DownloadFileAsync(string key);
    Task<Dictionary<FileTag, string>> GetTagsAsync(string key);
}