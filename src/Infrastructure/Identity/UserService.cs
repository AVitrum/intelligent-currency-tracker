using Amazon.S3;
using Amazon.S3.Model;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Utils;
using Domain.Common;
using Domain.Enums;
using Infrastructure.Identity.Results;
using Microsoft.AspNetCore.Identity;
using Shared.Dtos;
using Shared.Payload.Requests;

namespace Infrastructure.Identity;

public class UserService : IUserService
{
    private readonly string _bucketName;
    private readonly ILoginManagerFactory _loginManagerFactory;
    private readonly IMinioHelper _minioHelper;
    private readonly IAmazonS3 _s3Client;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(
        UserManager<ApplicationUser> userManager,
        ILoginManagerFactory loginManagerFactory,
        IAppSettings appSettings,
        IMinioHelper minioHelper)
    {
        _loginManagerFactory = loginManagerFactory;
        _s3Client = new AmazonS3Client(
            appSettings.AwsAccessKey,
            appSettings.AwsSecretKey,
            new AmazonS3Config
            {
                ServiceURL = appSettings.AwsEndpoint, ForcePathStyle = true
            });
        _minioHelper = minioHelper;
        _userManager = userManager;
        _bucketName = appSettings.AwsBucket;
    }

    public async Task<BaseResult> CreateAsync(CreateUserDto dto)
    {
        ApplicationUser newUser = new ApplicationUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber ?? string.Empty,
            CreationMethod = UserCreationMethod.EMAIL
        };

        IdentityResult creationResult = await _userManager.CreateAsync(newUser);

        if (!creationResult.Succeeded)
        {
            return BaseResult.FailureResult(creationResult.Errors.Select(e => e.Description).ToList());
        }

        IdentityResult passwordResult = await _userManager.AddPasswordAsync(newUser, dto.Password);

        if (!passwordResult.Succeeded)
        {
            return BaseResult.FailureResult(passwordResult.Errors.Select(e => e.Description).ToList());
        }

        IdentityResult roleResult = await _userManager.AddToRoleAsync(newUser, UserRole.USER.ToString());
        return roleResult.Succeeded
            ? BaseResult.SuccessResult()
            : BaseResult.FailureResult(roleResult.Errors.Select(e => e.Description).ToList());
    }

    public async Task<BaseResult> LoginAsync(LoginRequest request)
    {
        ILoginManager manager = _loginManagerFactory.Create(request.LoginProvider);
        return await manager.LoginAsync(request);
    }

    public async Task<BaseResult> LoginWithRefreshTokenAsync(RefreshTokenRequest request)
    {
        ILoginManager manager = _loginManagerFactory.Create(request.LoginProvider);
        return await manager.LoginWithRefreshTokenAsync(request);
    }

    public async Task<BaseResult> UploadPhotoAsync(string filePath, string fileExtension, string userId)
    {
        string prefix = $"users/{userId}/photo/{userId}";

        string? existingKey = await _minioHelper.FindKeyWithPrefixAsync(prefix);
        if (existingKey is not null)
        {
            await _s3Client.DeleteObjectAsync(_bucketName, existingKey);
        }

        string key = $"{prefix}{fileExtension}";

        PutObjectRequest putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            FilePath = filePath
        };

        await _s3Client.PutObjectAsync(putRequest);

        return BaseResult.SuccessResult();
    }

    public async Task<BaseResult> GetProfileAsync(string userId)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return BaseResult.FailureResult(new List<string> { "User not found" });
        }

        string? photoKey = await _minioHelper.FindKeyWithPrefixAsync($"users/{userId}/photo/{userId}");
        byte[] photoBytes = [];

        if (!string.IsNullOrEmpty(photoKey))
        {
            GetObjectRequest getRequest = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = photoKey
            };

            using GetObjectResponse? response = await _s3Client.GetObjectAsync(getRequest);
            await using Stream? stream = response.ResponseStream;
            using MemoryStream ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            photoBytes = ms.ToArray();
        }

        return ProfileResult.SuccessResult(userId, user.UserName!, user.Email!, user.PhoneNumber, photoBytes);
    }

    public async Task<BaseResult> ChangePasswordAsync(
        string oldPassword,
        string confirmPassword,
        string newPassword,
        string userId)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return BaseResult.FailureResult(["User not found"]);
        }

        IdentityResult result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        return !result.Succeeded
            ? BaseResult.FailureResult(result.Errors.Select(e => e.Description).ToList())
            : BaseResult.SuccessResult();
    }
}