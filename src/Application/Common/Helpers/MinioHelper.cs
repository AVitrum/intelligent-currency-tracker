using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Application.Common.Interfaces.Utils;
using Domain.Constants;
using Domain.Enums;

namespace Application.Common.Helpers;

public class MinioHelper : IMinioHelper
{
    private readonly string _bucketName;
    private readonly IAmazonS3 _s3Client;
    
    public MinioHelper(IAppSettings appSettings)
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
    
    public async Task<string> EnsureUniqueKeyAsync(string key)
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

    public async Task<bool> FileExistsAsync(string key)
    {
        try
        {
            GetObjectMetadataRequest metadataRequest = new GetObjectMetadataRequest
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

    public async Task<string?> FindKeyWithPrefixAsync(string prefix)
    {
        ListObjectsV2Request request = new ListObjectsV2Request
        {
            BucketName = _bucketName,
            Prefix = prefix
        };

        ListObjectsV2Response response = await _s3Client.ListObjectsV2Async(request);

        IEnumerable<S3Object> objects = response.S3Objects ?? Enumerable.Empty<S3Object>();
        return objects.FirstOrDefault()?.Key;
    }

    public void ConvertTypeIntoTags(string key, ref Dictionary<FileTag, string>? tags)
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