using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Utils;
using Application.Posts;
using Infrastructure.BackgroundServices;
using Infrastructure.Data.Repositories;
using Infrastructure.Email;
using Infrastructure.GoogleAuth;
using Infrastructure.Identity;
using Infrastructure.Identity.Factories;
using Infrastructure.Identity.Jwt;
using Infrastructure.Identity.Traceable;
using Infrastructure.Interfaces;
using Infrastructure.Minio;
using Infrastructure.Summary;
using Infrastructure.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
        services.AddScoped<IMinioService, MinioService>();
        services.AddScoped<ITraceableCurrencyService, TraceableCurrencyService>();
        services.AddScoped<ISummaryService, SummaryService>();
        services.AddScoped<IPostService, PostService>();

        //Repositories
        services.AddScoped<ICurrencyRepository, CurrencyRepository>();
        services.AddScoped<IRateRepository, RateRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUserSettingsRepository, UserSettingsRepository>();
        services.AddScoped<ITraceableCurrencyRepository, TraceableCurrencyRepository>();
        services.AddScoped<IFileLinkRepository, FileLinkRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IPostRepository, PostRepository>();

        //Background Services
        services.AddSingleton<ExchangeRateSyncService>();
        services.AddHostedService(provider => provider.GetRequiredService<ExchangeRateSyncService>());
        // services.AddHostedService<AiModelUpdateService>();
        // services.AddHostedService<AlertSenderService>();
        // services.AddHostedService<SummarySenderService>();

        return services;
    }
}