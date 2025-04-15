using Blazored.Toast;
using DevUI.Configurations;
using DevUI.Interfaces;
using DevUI.Services;

namespace DevUI;

public static class DependencyInjection
{
    public static void AddDevUI(this IServiceCollection services)
    {
        services.AddScoped<IDevUISettings, DevUISettings>();
        services.AddScoped<IHttpClientService, HttpClientService>();
        services.AddBlazoredToast();
    }
}