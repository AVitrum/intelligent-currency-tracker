namespace Shared.Payload.Responses;

public class ProfileResponse
{
    public string UserId { get; }
    public string UserName { get; }
    public string Email { get; }
    public string? PhoneNumber { get; }
    public byte[] Photo { get; }

    public ProfileResponse(string userId, string userName, string email, string? phoneNumber, byte[] photo)
    {
        UserId = userId;
        UserName = userName;
        Email = email;
        PhoneNumber = phoneNumber;
        Photo = photo;
    }
}