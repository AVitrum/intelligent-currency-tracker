using Domain.Common;

namespace Infrastructure.Identity.Results;

public class ProfileResult : BaseResult
{
    private ProfileResult(
        bool success,
        IEnumerable<string> errors,
        string userId,
        string userName,
        string email,
        string? phoneNumber,
        byte[] photo) : base(success, errors)
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

    public static ProfileResult SuccessResult(
        string userId,
        string userName,
        string email,
        string? phoneNumber,
        byte[] photo)
    {
        return new ProfileResult(true, [], userId, userName, email, phoneNumber, photo);
    }
}