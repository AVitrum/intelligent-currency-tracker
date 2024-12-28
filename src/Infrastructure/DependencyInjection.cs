using Infrastructure.ExternalApis.GoogleAuth;
using Infrastructure.ExternalApis.PrivateBank;
using Infrastructure.Identity;
using Infrastructure.Jwt;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    { 
        services.AddSingleton<IAppSettings, AppSettings>();
        
        var appSettings = services.BuildServiceProvider().GetRequiredService<IAppSettings>();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(appSettings.DbConnectionString));

        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
        
        services.AddHostedService<ExchangeRateFetcherService>();

        if (appSettings.IsDocker()) EnsureDatabaseCreated(services);
        
        return services;
    }

    private static void EnsureDatabaseCreated(IServiceCollection services)
    {
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        using IServiceScope scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
    }
}