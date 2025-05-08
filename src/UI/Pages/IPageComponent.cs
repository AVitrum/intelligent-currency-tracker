using Domain.Common;

namespace UI.Pages;

public interface IPageComponent
{
    Task HandleInvalidResponse(string message);
    Task<string> HandleResponse(BaseResponse? response);
}