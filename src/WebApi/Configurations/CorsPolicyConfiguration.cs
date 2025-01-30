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
                    .WithOrigins("http://localhost", "https://localhost:8003")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }
}