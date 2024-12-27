using Domain.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class ExchangeRateConfiguration: IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.Property(e => e.Currency)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<Currency>(v));
        
        builder.HasIndex(e => new { e.Date, e.Currency }).IsUnique();
    }
}