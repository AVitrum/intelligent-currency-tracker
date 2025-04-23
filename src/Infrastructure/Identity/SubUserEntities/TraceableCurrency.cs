using System.ComponentModel.DataAnnotations;
using Domain.Common;

namespace Infrastructure.Identity.SubUserEntities;

public class TraceableCurrency : BaseEntity
{
    public required Guid CurrencyId { get; init; }
    [MaxLength(512)] public required string UserId { get; init; }
    public Currency? Currency { get; set; }
    public ApplicationUser? User { get; set; }
}