using Domain.Common;

namespace Shared.Payload.Responses.Identity;

public class ProfileResponse : BaseResponse
{
    public ProfileResponse(
        bool success,
        string message,
        int statusCode,
        IEnumerable<string> errors,
        string userId,
        string userName,
        string email,
        string? phoneNumber,
        byte[] photo) : base(success, message, statusCode, errors)
    {
        UserId = userId;
        UserName = userName;
        Email = email;
        PhoneNumber = phoneNumber;
        Photo = photo;
    }

    public string UserId { get; }
    public string UserName { get; }
    public string Email { get; }
    public string? PhoneNumber { get; }
    public byte[] Photo { get; }
}