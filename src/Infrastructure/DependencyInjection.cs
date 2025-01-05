using Infrastructure.BackgroundServices;
using Infrastructure.Configuration;
using Infrastructure.Data.Repositories;
using Infrastructure.Email;
using Infrastructure.ExternalApis.GoogleAuth;
using Infrastructure.Identity;
using Infrastructure.Identity.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    { 
        services.AddSingleton<IAppSettings, AppSettings>();
        
        IAppSettings appSettings = services.BuildServiceProvider().GetRequiredService<IAppSettings>();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(appSettings.DbConnectionString));

        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        
        services.AddScoped<IUserFactory, UserFactory>();

        services.AddScoped<IEmailSender, EmailSender>();
        
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IIdentityAdminService, IdentityAdminService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
        
        services.AddHostedService<ExchangeRateFetcherService>();

        if (appSettings.IsDocker())
        {
            EnsureDatabaseCreated(services);
        }
        
        return services;
    }

    //TODO: Fix this method and move it to a separate class file.
    private static void EnsureDatabaseCreated(IServiceCollection services)
    {
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        using IServiceScope scope = serviceProvider.CreateScope();
        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (context.Database.CanConnect())
        {
            IEnumerable<string> pendingMigrations = context.Database.GetPendingMigrations();
            if (pendingMigrations.Any())
            {
                context.Database.Migrate();
            }
        }
        else
        {
            context.Database.EnsureCreated();
            IEnumerable<string> pendingMigrations = context.Database.GetPendingMigrations();
            if (pendingMigrations.Any())
            {
                context.Database.Migrate();
            }
        }
    }
}