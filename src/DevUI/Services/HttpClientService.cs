using System.Net;
using System.Net.Http.Json;
using DevUI.Helpers;
using DevUI.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.Payload.Requests;
using Shared.Payload.Responses;

namespace DevUI.Services;

public class HttpClientService : IHttpClientService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private readonly NavigationManager _navigation;
    private readonly IDevUISettings _settings;

    public HttpClientService(HttpClient http, IJSRuntime js, NavigationManager navigation, IDevUISettings settings)
    {
        _http = http;
        _js = js;
        _navigation = navigation;
        _settings = settings;
    }

    public async Task<HttpResponseMessage> SendRequestAsync(Func<Task<HttpResponseMessage>> requestFunc)
    {
        await JwtTokenHelper.SetJwtTokenInHeaderAsync(_http, _js, _navigation);
        var response = await requestFunc();

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var refreshed = await RefreshTokenAsync();
            if (refreshed) response = await requestFunc();
        }

        return response;
    }

    private async Task<bool> RefreshTokenAsync()
    {
        var refreshToken = await JwtTokenHelper.GetRefreshTokenFromLocalStorage(_js, _navigation);

        if (string.IsNullOrEmpty(refreshToken)) return false;

        var refreshRequest = new RefreshTokenRequest { RefreshToken = refreshToken, Provider = "DevUI" };
        var response = await _http.PostAsJsonAsync($"{_settings.ApiUrl}/Identity/refresh-token", refreshRequest);

        if (!response.IsSuccessStatusCode) return false;

        var responseContent = await response.Content.ReadFromJsonAsync<LoginResponse>();

        if (responseContent?.Token is null) return false;

        await JwtTokenHelper.SetJwtTokensInLocalStorageAsync(responseContent.Token, responseContent.RefreshToken, _js);
        await JwtTokenHelper.SetJwtTokenInHeaderAsync(_http, _js, _navigation);

        return true;
    }
}