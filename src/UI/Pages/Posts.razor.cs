using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Text;
using Blazored.Toast.Services;
using Domain.Common;
using Shared.Dtos;
using Shared.Payload.Responses;
using Shared.Payload.Responses.Post;
using UI.Common.Interfaces;
using UI.Services;
using IConfiguration = UI.Common.Interfaces.IConfiguration;

namespace UI.Pages;

public partial class Posts : ComponentBase, IPageComponent, IAsyncDisposable
{
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private IConfiguration Configuration { get; set; } = null!;
    [Inject] private IHttpClientService HttpClientService { get; set; } = null!;
    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private LocalizationService Localizer { get; set; } = null!;
    [Inject] private UserSettingsService UserSettingsService { get; set; } = null!;

    private List<PostDto> _posts = [];
    private bool _isLoading = true;

    private string _pageTitle = "";
    private string _headerTitle = "";
    private string _loadingPosts = "";
    private string _noPostsFound = "";
    private string _authorPrefix = "";
    private string _categoryPrefix = "";
    private string _errorLoadingPosts = "";
    private string _errorGeneric = "";
    private string _errorMessagePrefix = "";
    private string _errorStatusCodePrefix = "";
    private string _errorErrorsPrefix = "";
    private string _errorResponseNull = "";

    private readonly Dictionary<string, string> _categoryColors =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "General", "#6c757d" },
            { "News", "#007bff" },
            { "Updates", "#28a745" },
            { "Announcements", "#ffc107" },
            { "Analytics", "#17a2b8" },
            { "Predictions", "#dc3545" }
        };

    protected override async Task OnInitializedAsync()
    {
        await LoadLocalizedStringsAsync();
        UserSettingsService.OnSettingsChangedAsync += HandleSettingsChangedAsync;
        await LoadPostsAsync();
    }

    private async Task LoadLocalizedStringsAsync()
    {
        _pageTitle = await Localizer.GetStringAsync("posts.page_title");
        _headerTitle = await Localizer.GetStringAsync("posts.header.title");
        _loadingPosts = await Localizer.GetStringAsync("posts.loading_posts");
        _noPostsFound = await Localizer.GetStringAsync("posts.no_posts_found");
        _authorPrefix = await Localizer.GetStringAsync("posts.author_prefix");
        _categoryPrefix = await Localizer.GetStringAsync("posts.category_prefix");
        _errorLoadingPosts = await Localizer.GetStringAsync("posts.error.loading_posts");

        _errorGeneric = await Localizer.GetStringAsync("settings.toast.default_error_try_again");
        _errorMessagePrefix = await Localizer.GetStringAsync("settings.error.message_prefix");
        _errorStatusCodePrefix = await Localizer.GetStringAsync("settings.error.status_code_prefix");
        _errorErrorsPrefix = await Localizer.GetStringAsync("settings.error.errors_prefix");
        _errorResponseNull = await Localizer.GetStringAsync("contacts.error.response_null");
    }

    private async Task HandleSettingsChangedAsync()
    {
        await LoadLocalizedStringsAsync();
        StateHasChanged();
    }

    private async Task LoadPostsAsync(int page = 1, int pageSize = 10)
    {
        _isLoading = true;
        StateHasChanged();

        try
        {
            string apiUrl = $"{Configuration.ApiUrl}/Post/get-all?page={page}&pageSize={pageSize}";
            HttpResponseMessage response = await HttpClientService.SendRequestAsync(() => Http.GetAsync(apiUrl));

            if (response.IsSuccessStatusCode)
            {
                GetAllPostsResponse? postsResponse = await response.Content.ReadFromJsonAsync<GetAllPostsResponse>();
                if (postsResponse?.Success == true)
                {
                    _posts = postsResponse.Posts.ToList();
                }
                else
                {
                    string errorMessage = await HandleResponse(postsResponse);
                    await HandleInvalidResponse(errorMessage);
                    _posts = new List<PostDto>();
                }
            }
            else
            {
                DefaultResponse? errorResponse = null;
                try
                {
                    errorResponse = await response.Content.ReadFromJsonAsync<DefaultResponse>();
                }
                catch
                {
                    /* ignored */
                }

                string errorMessage = await HandleResponse(errorResponse);
                await HandleInvalidResponse($"{_errorLoadingPosts} {errorMessage}");
                _posts = [];
            }
        }
        catch (Exception ex)
        {
            await HandleInvalidResponse($"{_errorLoadingPosts} {ex.Message}");
            _posts = new List<PostDto>();
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private string GetCategoryColor(string category)
    {
        return _categoryColors.GetValueOrDefault(category, "#6c757d");
    }

    public Task<string> HandleResponse(BaseResponse? response)
    {
        if (response is null)
        {
            return Task.FromResult(_errorResponseNull);
        }

        StringBuilder errorMessage = new StringBuilder();
        errorMessage.AppendLine($"{_errorMessagePrefix} {response.Message}");
        errorMessage.AppendLine($"{_errorStatusCodePrefix} {response.StatusCode}");

        if (response.Errors.Any())
        {
            errorMessage.AppendLine($"{_errorErrorsPrefix} {string.Join(", ", response.Errors)}");
        }

        return Task.FromResult(errorMessage.ToString());
    }

    public Task HandleInvalidResponse(string? message = null)
    {
        ToastService.ShowError(message ?? _errorGeneric);
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        UserSettingsService.OnSettingsChangedAsync -= HandleSettingsChangedAsync;
        await Task.CompletedTask;
        GC.SuppressFinalize(this);
    }
}