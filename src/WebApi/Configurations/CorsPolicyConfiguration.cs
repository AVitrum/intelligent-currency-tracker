namespace WebApi.Configurations;

public static class CorsPolicyConfiguration
{
    public static void AddCustomCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("DevUI", builder =>
            {
                builder
                    .WithOrigins("https://localhost:8003", "https://host.docker.internal:8003", "http://localhost:5062",
                        "http://host.docker.internal:5062")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }
}