using Amazon.S3;
using Amazon.S3.Model;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Utils;
using Domain.Enums;

namespace Infrastructure.Minio;

public class MinioService : IMinioService
{
    private readonly string _bucketName;
    private readonly IMinioHelper _minioHelper;
    private readonly IAmazonS3 _s3Client;

    public MinioService(IAppSettings appSettings, IMinioHelper minioHelper)
    {
        _s3Client = new AmazonS3Client(
            appSettings.AwsAccessKey,
            appSettings.AwsSecretKey,
            new AmazonS3Config
            {
                ServiceURL = appSettings.AwsEndpoint, ForcePathStyle = true
            });
        _bucketName = appSettings.AwsBucket;
        _minioHelper = minioHelper;
    }

    public async Task<string> UploadFileAsync(string filePath, string key, Dictionary<FileTag, string>? tags = null)
    {
        key = key.Replace(" ", "_");
        key = await _minioHelper.EnsureUniqueKeyAsync(key);

        _minioHelper.ConvertTypeIntoTags(key, ref tags);

        PutObjectRequest putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            FilePath = filePath
        };

        await _s3Client.PutObjectAsync(putRequest);

        if (tags is not null && tags.Count != 0)
        {
            List<Tag> tagSet = tags.Select(kvp => new Tag { Key = kvp.Key.ToString(), Value = kvp.Value }).ToList();
            PutObjectTaggingRequest tagRequest = new PutObjectTaggingRequest
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
        GetObjectRequest request = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        };

        using GetObjectResponse? response = await _s3Client.GetObjectAsync(request);
        using MemoryStream memoryStream = new MemoryStream();
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
}