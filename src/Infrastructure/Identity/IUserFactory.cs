using Domain.Common;

namespace Infrastructure.Identity;

public interface IUserFactory
{
    Task<BaseResult> CreateUserAsync(UserFactoryDelegate userFactory, PostCreationDelegate? postCreationDelegate = null);
}