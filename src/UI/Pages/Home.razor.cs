using Microsoft.AspNetCore.Components;

namespace UI.Pages;

public partial class Home
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private void NavigateToDashboard()
    {
        NavigationManager.NavigateTo("/dashboard");
    }

    private void NavigateToRegister()
    {
        NavigationManager.NavigateTo("/auth");
    }
}