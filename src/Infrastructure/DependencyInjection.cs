using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Utils;
using Infrastructure.BackgroundServices;
using Infrastructure.Configuration;
using Infrastructure.Data.Repositories;
using Infrastructure.Email;
using Infrastructure.ExternalApis.GoogleAuth;
using Infrastructure.Identity;
using Infrastructure.Identity.Factories;
using Infrastructure.Identity.Jwt;
using Infrastructure.Utils;
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

        services.AddHttpContextAccessor();

        //Utils
        services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<IUserHelper, UserHelper>();

        //No Interface
        services.AddScoped<DefaultLoginManager>();
        services.AddScoped<DevUILoginManager>();

        //Factories
        services.AddScoped<ILoginManagerFactory, LoginManagerFactory>();

        //Services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        services.AddScoped<IJwtService, JwtService>();

        //Repositories
        services.AddScoped<ICurrencyRepository, CurrencyRepository>();
        services.AddScoped<IRateRepository, RateRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        //Background Services
        services.AddHostedService<ExchangeRateSyncService>();

        if (appSettings.IsDocker()) EnsureDatabaseCreated(services);

        return services;
    }

    //TODO: Fix this method and move it to a separate class file.
    private static void EnsureDatabaseCreated(IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (context.Database.CanConnect())
        {
            var pendingMigrations = context.Database.GetPendingMigrations();
            if (pendingMigrations.Any()) context.Database.Migrate();
        }
        else
        {
            context.Database.EnsureCreated();
            var pendingMigrations = context.Database.GetPendingMigrations();
            if (pendingMigrations.Any()) context.Database.Migrate();
        }
    }
}