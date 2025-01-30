using Domain.Common;
using Shared.Payload.Requests;

namespace Application.Common.Interfaces;

public interface ILoginManager
{
    Task<BaseResult> LoginAsync(LoginRequest request);
}