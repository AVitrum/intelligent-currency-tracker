using Microsoft.AspNetCore.Authentication.Cookies;

namespace WebApi;

public static class DependencyInjection
{
    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
        var issuer = isDocker ? configuration["JWT_ISSUER"] : configuration["Jwt:Issuer"];
        var audience = isDocker ? configuration["JWT_AUDIENCE"] : configuration["Jwt:Audience"];
        var key = isDocker ? configuration["JWT_KEY"] : configuration["Jwt:Key"];
        var googleClientId = isDocker ? configuration["GOOGLE_CLIENT_ID"] : configuration["Authentication:Google:ClientId"];
        var googleClientSecret = isDocker ? configuration["GOOGLE_CLIENT_SECRET"] : configuration["Authentication:Google:ClientSecret"];

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
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!))
                };
            })
            //TODO: Add custom exceptions
            .AddGoogle(options =>
            {
                options.ClientId = googleClientId ?? throw new Exception();
                options.ClientSecret = googleClientSecret ?? throw new Exception();
                options.SaveTokens = true;
            });;

        return services;
    }
    
}