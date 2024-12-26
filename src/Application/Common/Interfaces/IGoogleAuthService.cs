using Domain.Common;
using Microsoft.AspNetCore.Authentication;

namespace Application.Common.Interfaces;

public interface IGoogleAuthService
{
    Task<BaseResult> HandleGoogleResponse(AuthenticateResult authResult);
}