using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
{
    public void Configure(EntityTypeBuilder<UserSettings> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<UserSettings>(x => x.UserId)
            .IsRequired();
    }
}