using Infrastructure.Identity.SubUserEntities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class TraceableCurrencyConfiguration : IEntityTypeConfiguration<TraceableCurrency>
{
    public void Configure(EntityTypeBuilder<TraceableCurrency> builder)
    {
        builder.HasKey(uc => new { uc.CurrencyId, uc.UserId });

        builder.HasOne(uc => uc.User)
            .WithMany(u => u.TrackedCurrencies)
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}