using Application.Common.Payload.Requests;
using Domain.Common;

namespace Application.Common.Interfaces;

public interface IIdentityAdminService
{
    Task<BaseResult> ProvideAdminFunctionality(ProvideAdminFunctionalityRequest request);
}