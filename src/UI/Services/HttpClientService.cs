using System.Net;
using System.Net.Http.Json;
using Domain.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.Helpers;
using Shared.Payload.Requests;
using Shared.Payload.Responses.Identity;
using UI.Configurations;

namespace UI.Services;

public class HttpClientService : IHttpClientService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private readonly NavigationManager _navigation;
    private readonly IUISettings _settings;

    public HttpClientService(
        HttpClient http,
        IJSRuntime js,
        NavigationManager navigation,
        IUISettings settings)
    {
        _http = http;
        _js = js;
        _navigation = navigation;
        _settings = settings;
    }

    public async Task<HttpResponseMessage> SendRequestAsync(Func<Task<HttpResponseMessage>> requestFunc)
    {
        await JwtTokenHelper.SetJwtTokenInHeaderAsync(_http, _js, _navigation);
        HttpResponseMessage response = await requestFunc();

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            bool refreshed = await RefreshTokenAsync();

            if (refreshed)
            {
                response = await requestFunc();
            }
            else
            {
                await JwtTokenHelper.RemoveJwtTokensFromCookiesAsync(_js, _navigation);
            }
        }

        return response;
    }

    private async Task<bool> RefreshTokenAsync()
    {
        string refreshToken = await JwtTokenHelper.GetRefreshTokenFromCookies(_js, _navigation);

        if (string.IsNullOrEmpty(refreshToken))
        {
            return false;
        }

        RefreshTokenRequest refreshRequest = new RefreshTokenRequest
            { RefreshToken = refreshToken, Provider = nameof(LoginManagerProvider.Default) };
        HttpResponseMessage response =
            await _http.PostAsJsonAsync($"{_settings.ApiUrl}/Identity/refresh-token", refreshRequest);

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        LoginResponse? responseContent = await response.Content.ReadFromJsonAsync<LoginResponse>();

        if (responseContent?.Token is null)
        {
            return false;
        }

        await JwtTokenHelper.SetJwtTokensInCookiesAsync(responseContent.Token, responseContent.RefreshToken, _js);
        await JwtTokenHelper.SetJwtTokenInHeaderAsync(_http, _js, _navigation);

        return true;
    }
}