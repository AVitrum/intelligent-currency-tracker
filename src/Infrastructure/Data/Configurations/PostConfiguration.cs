using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).IsRequired();

        builder.Property(p => p.Category)
            .HasConversion<string>()
            .IsRequired();
        
        builder.Property(p => p.Language)
            .HasConversion<string>()
            .IsRequired();

        builder.HasMany(r => r.Attachments)
            .WithOne();
    }
}