using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class FileLinkConfiguration : IEntityTypeConfiguration<FileLink>
{
    public void Configure(EntityTypeBuilder<FileLink> builder) { }
}