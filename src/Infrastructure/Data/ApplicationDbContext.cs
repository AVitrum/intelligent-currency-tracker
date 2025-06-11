using Infrastructure.Identity;
using Infrastructure.Identity.SubUserEntities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<Rate> Rates => Set<Rate>();
    public DbSet<ApiRequestLog> ApiRequestLogs => Set<ApiRequestLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();
    public DbSet<TraceableCurrency> TraceableCurrencies => Set<TraceableCurrency>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<FileLink> FileLinks => Set<FileLink>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}