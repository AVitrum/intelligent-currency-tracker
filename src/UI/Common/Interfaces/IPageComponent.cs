using Domain.Common;

namespace UI.Common.Interfaces;

public interface IPageComponent
{
    Task HandleInvalidResponse(string message);
    Task<string> HandleResponse(BaseResponse? response);
}