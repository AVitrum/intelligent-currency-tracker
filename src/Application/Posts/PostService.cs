using Amazon.S3;
using Amazon.S3.Model;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Utils;
using Application.Posts.Results;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Shared.Dtos;
using Shared.Payload.Requests;

namespace Application.Posts;

public class PostService : IPostService
{
    private readonly string _bucketName;
    private readonly ILogger<PostService> _logger;
    private readonly IAmazonS3 _s3Client;
    private readonly IPostRepository _postRepository;
    private readonly IUserService _userService;

    public PostService(
        ILogger<PostService> logger,
        IPostRepository postRepository,
        IUserService userService,
        IAppSettings appSettings)
    {
        _logger = logger;
        _postRepository = postRepository;
        _userService = userService;

        _bucketName = appSettings.AwsBucket;
        _s3Client = new AmazonS3Client(
            appSettings.AwsAccessKey,
            appSettings.AwsSecretKey,
            new AmazonS3Config
            {
                ServiceURL = appSettings.AwsEndpoint, ForcePathStyle = true
            });
    }

    public async Task<BaseResult> CreateAsync(CreatePostRequest request)
    {
        Post post = new Post
        {
            Title = request.Title,
            Content = request.Content,
            Category = Enum.Parse<PostCategory>(request.Category, true),
            UserId = request.AuthorId
        };

        const string path = "posts/files/";

        if (request.Attachments is not null && request.Attachments.Count != 0)
        {
            foreach ((string filePath, string fileExtension) in request.Attachments)
            {
                string key = $"{path}{Guid.NewGuid()}{fileExtension}";

                try
                {
                    PutObjectRequest putRequest = new PutObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = key,
                        FilePath = filePath
                    };

                    await _s3Client.PutObjectAsync(putRequest);
                    _logger.LogInformation("File {FilePath} uploaded successfully to {Key}", filePath, key);

                    FileLink fileLink = new FileLink
                    {
                        Key = key,
                        Extension = fileExtension
                    };
                    post.Attachments.Add(fileLink);
                }
                catch (AmazonS3Exception e)
                {
                    _logger.LogError(e, "An error occurred while uploading file {FilePath} to S3", filePath);
                    BaseResult.FailureResult(["Error uploading file to S3: " + e.Message]);
                }
            }
        }

        await _postRepository.AddAsync(post);
        _logger.LogInformation("Post created successfully");
        return BaseResult.SuccessResult();
    }

    public async Task<BaseResult> GetById(Guid id)
    {
        Post post = await _postRepository.GetByIdAsync(id);
        BaseResult convertUserToDtoResult = await _userService.ConvertUserToDtoAsync(post.UserId);
        if (convertUserToDtoResult is not ConvertUserToDtoResult extendedResult)
        {
            _logger.LogError("Failed to convert user to DTO for post with ID {PostId}", id);
            return BaseResult.FailureResult(["Failed to convert user to DTO"]);
        }

        UserDto authorDto = extendedResult.User;

        PostDto postDto = new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            Category = post.Category.ToString(),
            AuthorId = post.UserId,
            Author = authorDto
        };

        List<FileLink> attachments = (await _postRepository.GetAttachmentsByPostIdAsync(post.Id)).ToList();
        if (attachments.Count != 0)
        {
            List<byte[]> fileBytesList = [];

            foreach (FileLink attachment in post.Attachments)
            {
                string key = attachment.Key;

                GetObjectRequest getRequest = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };
                using GetObjectResponse? response = await _s3Client.GetObjectAsync(getRequest);
                await using Stream? stream = response.ResponseStream;
                using MemoryStream ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                byte[] fileBytes = ms.ToArray();
                fileBytesList.Add(fileBytes);
            }

            postDto.Attachments = fileBytesList;
        }

        return GetPostByIdResult.SuccessResult(postDto);
    }

    public async Task<BaseResult> GetAllAsync(int page, int pageSize)
    {
        List<Post> posts = (await _postRepository.GetAllAsync(page, pageSize)).ToList();
        if (posts.Count == 0)
        {
            _logger.LogInformation("No posts found");
            return BaseResult.FailureResult(["No posts found"]);
        }

        List<PostDto> postDtos = [];
        foreach (Post post in posts)
        {
            BaseResult convertUserToDtoResult = await _userService.ConvertUserToDtoAsync(post.UserId);
            if (convertUserToDtoResult is not ConvertUserToDtoResult extendedResult)
            {
                _logger.LogError("Failed to convert user to DTO for post with ID {PostId}", post.Id);
                return BaseResult.FailureResult(["Failed to convert user to DTO"]);
            }

            UserDto authorDto = extendedResult.User;

            PostDto postDto = new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                Category = post.Category.ToString(),
                AuthorId = post.UserId,
                Author = authorDto
            };

            postDtos.Add(postDto);
        }

        return GetAllPostsResult.SuccessResult(postDtos);
    }
}