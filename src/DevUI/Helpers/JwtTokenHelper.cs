using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DevUI.Helpers;

public static class JwtTokenHelper
{
    private static async Task EnsureJsFunctionsExistAsync(IJSRuntime js)
    {
        await js.InvokeVoidAsync("eval",
            """
            
                        window.setCookie = function(name, value, days) {
                            let expires = '';
                            if (days) {
                                let date = new Date();
                                date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
                                expires = '; expires=' + date.toUTCString();
                            }
                            document.cookie = name + '=' + value + expires + '; path=/';
                        };
            
                        window.getCookie = function(name) {
                            let nameEQ = name + '=';
                            let ca = document.cookie.split(';');
                            for (let i = 0; i < ca.length; i++) {
                                let c = ca[i];
                                while (c.charAt(0) == ' ') c = c.substring(1, c.length);
                                if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
                            }
                            return null;
                        };
            
                        window.deleteCookie = function(name) {
                            document.cookie = name + '=; Expires=Thu, 01 Jan 1970 00:00:01 GMT; Path=/;';
                        };
                    
            """);
    }

    public static async Task SetJwtTokensInCookiesAsync(string jwtToken, string refreshToken, IJSRuntime js)
    {
        await EnsureJsFunctionsExistAsync(js);
        await js.InvokeVoidAsync("setCookie", "jwtToken", jwtToken, 7);
        await js.InvokeVoidAsync("setCookie", "refreshToken", refreshToken, 7);
    }

    public static async Task RemoveJwtTokensFromCookiesAsync(IJSRuntime js, NavigationManager navigation)
    {
        await EnsureJsFunctionsExistAsync(js);
        await js.InvokeVoidAsync("deleteCookie", "jwtToken");
        await js.InvokeVoidAsync("deleteCookie", "refreshToken");
        navigation.NavigateTo("/", true);
    }

    public static async Task SetJwtTokenInHeaderAsync(HttpClient http, IJSRuntime js, NavigationManager navigation)
    {
        await EnsureJsFunctionsExistAsync(js);
        string? jwtToken = await js.InvokeAsync<string?>("getCookie", "jwtToken");
        if (string.IsNullOrEmpty(jwtToken))
        {
            navigation.NavigateTo("/", true);
            return;
        }

        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
    }

    public static async Task<string?> GetJwtTokenFromCookies(IJSRuntime js, NavigationManager navigation)
    {
        await EnsureJsFunctionsExistAsync(js);
        string? token = await js.InvokeAsync<string?>("getCookie", "jwtToken");
        return token;
    }

    public static async Task<string> GetRefreshTokenFromCookies(IJSRuntime js, NavigationManager navigation)
    {
        await EnsureJsFunctionsExistAsync(js);
        string? token = await js.InvokeAsync<string?>("getCookie", "refreshToken");

        if (string.IsNullOrEmpty(token))
        {
            await RemoveJwtTokensFromCookiesAsync(js, navigation);
        }

        return token!;
    }
}