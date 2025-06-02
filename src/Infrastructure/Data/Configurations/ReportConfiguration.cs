using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.HasMany(e => e.Attachments)
            .WithOne()
            .IsRequired();
    }
}