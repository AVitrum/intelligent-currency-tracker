using Domain.Enums;
using Infrastructure.ExternalApis.GoogleAuth;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Identity;

public class UserFactory : IUserFactory
{
    private readonly IServiceProvider _serviceProvider;

    public UserFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IUserService Create(UserServiceType type)
    {
        return type switch
        {
            UserServiceType.GOOGLE => _serviceProvider.GetRequiredService<GoogleUserService>(),
            UserServiceType.EMAIL => _serviceProvider.GetRequiredService<UserService>(),
            _ => throw new ArgumentException("Invalid type for user service")
        };
    }
}