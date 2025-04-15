using Application.Common.Interfaces.Utils;
using Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Identity.Factories;

public class LoginManagerFactory : ILoginManagerFactory
{
    private readonly IServiceProvider _serviceProvider;

    public LoginManagerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ILoginManager Create(LoginManagerProvider provider)
    {
        return provider switch
        {
            LoginManagerProvider.Default => _serviceProvider.GetRequiredService<DefaultLoginManager>(),
            LoginManagerProvider.DevUI => _serviceProvider.GetRequiredService<DevUILoginManager>(),
            _ => throw new ArgumentException("Invalid type for login manager")
        };
    }
}