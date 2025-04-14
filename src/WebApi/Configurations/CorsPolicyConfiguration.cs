namespace WebApi.Configurations;

public static class CorsPolicyConfiguration
{
    public static void AddCustomCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowedCors", builder =>
            {
                builder
                    .WithOrigins(
                        "http://localhost",
                        "https://localhost:8003",
                        "http://localhost:5097",
                        "https://localhost:7135")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }
}