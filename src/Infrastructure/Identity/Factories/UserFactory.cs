using Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Identity.Factories;

public class UserFactory : IUserFactory
{
    private readonly IServiceProvider _serviceProvider;

    public UserFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IUserService Create(UserServiceProvider provider)
    {
        return provider switch
        {
            UserServiceProvider.DEFAULT => _serviceProvider.GetRequiredService<UserService>(),
            UserServiceProvider.ADMIN => _serviceProvider.GetRequiredService<AdminUserService>(),
            _ => throw new ArgumentException("Invalid type for user service")
        };
    }
}