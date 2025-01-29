using Domain.Enums;

namespace Application.Common.Interfaces;

public interface ILoginManagerFactory
{
    ILoginManager Create(LoginManagerProvider provider);
}