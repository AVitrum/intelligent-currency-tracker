using Domain.Enums;
using Infrastructure.Identity.SubUserEntities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public required UserCreationMethod CreationMethod { get; set; }

    public ICollection<TraceableCurrency> TrackedCurrencies { get; set; } = new List<TraceableCurrency>();
}