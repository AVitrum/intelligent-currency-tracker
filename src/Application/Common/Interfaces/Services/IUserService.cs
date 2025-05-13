using Domain.Common;
using Shared.Dtos;
using Shared.Payload.Requests;

namespace Application.Common.Interfaces.Services;

public interface IUserService
{
    Task<BaseResult> CreateAsync(CreateUserDto dto);
    Task<BaseResult> LoginAsync(LoginRequest request);
    Task<BaseResult> LoginWithRefreshTokenAsync(RefreshTokenRequest request);
    Task<BaseResult> UploadPhotoAsync(string filePath, string fileExtension, string userId);
    Task<BaseResult> GetProfileAsync(string userId);
    Task<BaseResult> SaveSettingsAsync(SettingsDto dto, string userId);
    Task<BaseResult> GetSettingsAsync(string userId);
    Task<BaseResult> ChangePasswordAsync(string oldPassword, string confirmPassword, string newPassword, string userId);
}