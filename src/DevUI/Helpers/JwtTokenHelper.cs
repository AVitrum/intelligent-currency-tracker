using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DevUI.Helpers;

public static class JwtTokenHelper
{
    public static async Task SetJwtTokensInLocalStorageAsync(string jwtToken, string refreshToken, IJSRuntime js)
    {
        await js.InvokeVoidAsync("localStorage.setItem", "jwtToken", jwtToken);
        await js.InvokeVoidAsync("localStorage.setItem", "refreshToken", refreshToken);
    }

    public static async Task RemoveJwtTokensFromLocalStorageAsync(IJSRuntime js, NavigationManager navigation)
    {
        await js.InvokeVoidAsync("localStorage.removeItem", "jwtToken");
        await js.InvokeVoidAsync("localStorage.removeItem", "refreshToken");
        navigation.NavigateTo("/", true);
    }

    public static async Task SetJwtTokenInHeaderAsync(HttpClient http, IJSRuntime js, NavigationManager navigation)
    {
        var jwtToken = await js.InvokeAsync<string?>("localStorage.getItem", "jwtToken");
        if (string.IsNullOrEmpty(jwtToken))
        {
            navigation.NavigateTo("/", true);
            return;
        }

        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
    }

    public static async Task<string> GetRefreshTokenFromLocalStorage(IJSRuntime js, NavigationManager navigation)
    {
        var token = await js.InvokeAsync<string?>("localStorage.getItem", "refreshToken");

        if (token is null) await RemoveJwtTokensFromLocalStorageAsync(js, navigation);

        return token!;
    }
}