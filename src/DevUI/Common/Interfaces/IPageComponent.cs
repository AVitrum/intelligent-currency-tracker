using Domain.Common;

namespace DevUI.Common.Interfaces;

public interface IPageComponent
{
    Task HandleInvalidResponse(string message);
    Task<string> HandleResponse(BaseResponse? response);
}