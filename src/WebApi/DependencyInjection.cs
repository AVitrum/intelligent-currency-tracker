using Microsoft.AspNetCore.Authentication.Cookies;

namespace WebApi;

public static class DependencyInjection
{
    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services)
    {
        var appSettings = services.BuildServiceProvider().GetRequiredService<IAppSettings>();
        
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = appSettings.JwtIssuer,
                    ValidAudience = appSettings.JwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.JwtKey))
                };
            })
            .AddGoogle(options =>
            {
                options.ClientId = appSettings.GoogleClientId ?? throw new Exception("Google Client ID is missing.");
                options.ClientSecret = appSettings.GoogleClientSecret ?? throw new Exception("Google Client Secret is missing.");
                options.SaveTokens = true;
            });

        return services;
    }
}