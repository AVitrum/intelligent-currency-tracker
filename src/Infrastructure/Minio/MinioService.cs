using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Utils;
using Domain.Constants;
using Domain.Enums;

namespace Infrastructure.Minio;

public class MinioService : IMinioService
{
    private readonly string _bucketName;
    private readonly IAmazonS3 _s3Client;

    public MinioService(IAppSettings appSettings)
    {
        _s3Client = new AmazonS3Client(
            appSettings.AwsAccessKey,
            appSettings.AwsSecretKey,
            new AmazonS3Config
            {
                ServiceURL = appSettings.AwsEndpoint, ForcePathStyle = true
            });
        _bucketName = appSettings.AwsBucket;
    }

    public async Task<string> UploadFileAsync(string filePath, string key, Dictionary<FileTag, string>? tags = null)
    {
        key = key.Replace(" ", "_");
        key = await EnsureUniqueKeyAsync(key);

        ConvertTypeIntoTags(key, ref tags);

        PutObjectRequest putRequest = new()
        {
            BucketName = _bucketName,
            Key = key,
            FilePath = filePath
        };

        await _s3Client.PutObjectAsync(putRequest);

        if (tags is not null && tags.Count != 0)
        {
            List<Tag> tagSet = tags.Select(kvp => new Tag { Key = kvp.Key.ToString(), Value = kvp.Value }).ToList();
            PutObjectTaggingRequest tagRequest = new()
            {
                BucketName = _bucketName,
                Key = key,
                Tagging = new Tagging { TagSet = tagSet }
            };

            await _s3Client.PutObjectTaggingAsync(tagRequest);
        }

        return key;
    }

    public async Task<byte[]> DownloadFileAsync(string key)
    {
        GetObjectRequest request = new()
        {
            BucketName = _bucketName,
            Key = key
        };

        using GetObjectResponse? response = await _s3Client.GetObjectAsync(request);
        using MemoryStream memoryStream = new();
        await response.ResponseStream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    public async Task<Dictionary<FileTag, string>> GetTagsAsync(string key)
    {
        key = key.Replace(" ", "_");

        GetObjectTaggingResponse? tagResponse = await _s3Client.GetObjectTaggingAsync(new GetObjectTaggingRequest
        {
            BucketName = _bucketName,
            Key = key
        });

        if (tagResponse.Tagging == null || tagResponse.Tagging.Count == 0)
        {
            return new Dictionary<FileTag, string>();
        }

        return tagResponse.Tagging.ToDictionary(t => Enum.Parse<FileTag>(t.Key), t => t.Value);
    }

    private async Task<string> EnsureUniqueKeyAsync(string key)
    {
        int counter = 1;
        string extension = Path.GetExtension(key);
        string baseName = Path.GetFileNameWithoutExtension(key);

        while (await FileExistsAsync(key))
        {
            key = $"{baseName}_{counter}{extension}";
            counter++;
        }

        return key;
    }

    private async Task<bool> FileExistsAsync(string key)
    {
        try
        {
            GetObjectMetadataRequest metadataRequest = new()
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.GetObjectMetadataAsync(metadataRequest);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    private static void ConvertTypeIntoTags(string key, ref Dictionary<FileTag, string>? tags)
    {
        tags ??= new Dictionary<FileTag, string>();

        string extension = Path.GetExtension(key);

        if (!string.IsNullOrEmpty(extension))
        {
            extension = extension.TrimStart('.');

            tags.TryAdd(FileTag.Extension, extension);

            if (!tags.ContainsKey(FileTag.FileType))
            {
                if (FileConstants.ExtensionToType.TryGetValue(extension, out string? value))
                {
                    tags.Add(FileTag.FileType, value);
                }
            }
        }
    }
}