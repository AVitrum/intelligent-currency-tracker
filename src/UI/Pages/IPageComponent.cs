namespace UI.Pages;

public interface IPageComponent
{
    Task HandleInvalidResponse(string message);
}