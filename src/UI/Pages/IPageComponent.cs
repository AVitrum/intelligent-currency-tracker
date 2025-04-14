using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;

namespace UI.Pages;

public interface IPageComponent
{
    Task HandleInvalidResponse(string message);
}