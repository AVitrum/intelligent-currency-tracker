using Domain.Common;
using Microsoft.AspNetCore.Authentication;

namespace Application.Common.Interfaces.Services;

public interface IGoogleAuthService
{
    Task<BaseResult> HandleGoogleResponse(AuthenticateResult authResult);
}