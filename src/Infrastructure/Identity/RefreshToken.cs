using Domain.Common;

namespace Infrastructure.Identity;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public DateTime ExpiresOnUtc { get; set; }

    public ApplicationUser? User { get; set; }
}