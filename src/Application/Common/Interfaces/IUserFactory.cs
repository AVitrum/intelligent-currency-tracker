using Domain.Enums;

namespace Application.Common.Interfaces;

public interface IUserFactory
{
    IUserService Create(UserServiceType type);
}