using Domain.Enums;

namespace Application.Common.Interfaces.Utils;

public interface IMinioHelper
{
    Task<string> EnsureUniqueKeyAsync(string key);
    Task<bool> FileExistsAsync(string key);
    Task<string?> FindKeyWithPrefixAsync(string prefix);
    void ConvertTypeIntoTags(string key, ref Dictionary<FileTag, string>? tags);
}