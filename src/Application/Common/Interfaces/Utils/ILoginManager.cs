using Domain.Common;
using Shared.Payload.Requests;

namespace Application.Common.Interfaces.Utils;

public interface ILoginManager
{
    Task<BaseResult> LoginAsync(LoginRequest request);
    Task<BaseResult> LoginWithRefreshTokenAsync(RefreshTokenRequest request);
}