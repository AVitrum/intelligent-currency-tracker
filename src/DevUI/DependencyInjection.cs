using Blazored.Toast;
using DevUI.Configurations;

namespace DevUI;

public static class DependencyInjection
{
    public static IServiceCollection AddDevUI(this IServiceCollection services)
    {
        services.AddScoped<IDevUISettings, DevUISettings>();
        services.AddBlazoredToast();
        return services;
    }
}