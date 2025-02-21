using Domain.Enums;

namespace Application.Common.Interfaces.Utils;

public interface ILoginManagerFactory
{
    ILoginManager Create(LoginManagerProvider provider);
}