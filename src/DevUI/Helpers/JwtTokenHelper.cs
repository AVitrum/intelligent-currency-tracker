using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DevUI.Helpers;

public static class JwtTokenHelper
{
    public static async Task SetJwtTokenInLocalStorageAsync(string jwtToken, IJSRuntime js) =>
        await js.InvokeVoidAsync("localStorage.setItem", "jwtToken", jwtToken);
    
    public static async Task SetJwtTokenInHeaderAsync(HttpClient http, IJSRuntime js, NavigationManager navigation)
    {
        var jwtToken = await js.InvokeAsync<string?>("localStorage.getItem", "jwtToken");
        if (string.IsNullOrEmpty(jwtToken))
        {
            navigation.NavigateTo("/login");
            return;
        }

        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
    }
}