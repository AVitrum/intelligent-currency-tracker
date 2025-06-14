using System.Net;
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

public partial class PostDetails : ComponentBase, IPageComponent, IAsyncDisposable
{
    [Parameter] public Guid Id { get; set; }

    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private IConfiguration Configuration { get; set; } = null!;
    [Inject] private IHttpClientService HttpClientService { get; set; } = null!;
    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private LocalizationService Localizer { get; set; } = null!;
    [Inject] private UserSettingsService UserSettingsService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

    private PostDto? _post;
    private List<string>? _attachmentsData; 
    private bool _isLoadingPost = true;
    private bool _isLoadingAttachments;
    private string? _errorMessage;

    private string _pageTitleFormat = "";
    private string _loadingPost = "";
    private string _postNotFound = "";
    private string _errorLoadingPost = "";
    private string _labelCategory = "";
    private string _labelAuthor = "";
    private string _labelPublishedOn = "";
    private string _labelContent = "";
    private string _labelAttachments = "";
    private string _closePreviewButtonArialLabel = "";
    private string _buttonBackToPosts = "";

    private string _errorGeneric = "";
    private string _errorMessagePrefix = "";
    private string _errorStatusCodePrefix = "";
    private string _errorErrorsPrefix = "";
    private string _errorResponseNull = "";

    private string? _selectedAttachmentPreview; 

    private readonly Dictionary<string, string> _categoryColors =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "General", "#6c757d" },
            { "News", "#007bff" },
            { "Technology", "#17a2b8" },
            { "Health", "#28a745" },
            { "Science", "#ffc107" },
            { "Business", "#dc3545" },
            { "Travel", "#6f42c1" },
            { "Lifestyle", "#e83e8c" }
        };

    private const string DefaultCategoryColor = "#6c757d";

    protected override async Task OnInitializedAsync()
    {
        await LoadLocalizedStringsAsync();
        UserSettingsService.OnSettingsChangedAsync += HandleSettingsChangedAsync;
    }

    protected override async Task OnParametersSetAsync()
    {
        if (Id != Guid.Empty)
        {
            await LoadPostAsync();
        }
        else
        {
            _isLoadingPost = false;
            _errorMessage = await Localizer.GetStringAsync("postdetails.post_not_found");
        }
    }

    private async Task LoadLocalizedStringsAsync()
    {
        _pageTitleFormat = await Localizer.GetStringAsync("postdetails.page_title_format");
        _loadingPost = await Localizer.GetStringAsync("postdetails.loading_post");
        _postNotFound = await Localizer.GetStringAsync("postdetails.post_not_found");
        _errorLoadingPost = await Localizer.GetStringAsync("postdetails.error.loading_post");
        _labelCategory = await Localizer.GetStringAsync("postdetails.label.category");
        _labelAuthor = await Localizer.GetStringAsync("postdetails.label.author");
        _labelPublishedOn = await Localizer.GetStringAsync("postdetails.label.published_on");
        _labelContent = await Localizer.GetStringAsync("postdetails.label.content");
        _labelAttachments = await Localizer.GetStringAsync("postdetails.label.attachments");
        _closePreviewButtonArialLabel = await Localizer.GetStringAsync("postdetails.button.close_preview_aria_label");
        _buttonBackToPosts = await Localizer.GetStringAsync("postdetails.button.back_to_posts");

        _errorGeneric = await Localizer.GetStringAsync("settings.toast.default_error_try_again");
        _errorMessagePrefix = await Localizer.GetStringAsync("settings.error.message_prefix");
        _errorStatusCodePrefix = await Localizer.GetStringAsync("settings.error.status_code_prefix");
        _errorErrorsPrefix = await Localizer.GetStringAsync("settings.error.errors_prefix");
        _errorResponseNull = await Localizer.GetStringAsync("contacts.error.response_null");

        if (_post != null || _selectedAttachmentPreview != null || _attachmentsData != null || !_isLoadingPost ||
            !_isLoadingAttachments)
        {
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task HandleSettingsChangedAsync()
    {
        await LoadLocalizedStringsAsync();
    }

    private async Task LoadPostAsync()
    {
        _isLoadingPost = true;
        _errorMessage = null;
        _post = null;
        _attachmentsData = null;
        _selectedAttachmentPreview = null;
        StateHasChanged();

        try
        {
            string apiUrl = $"{Configuration.ApiUrl}/Post/get-by-id/{Id}";
            HttpResponseMessage response = await HttpClientService.SendRequestAsync(() => Http.GetAsync(apiUrl));

            if (response.IsSuccessStatusCode)
            {
                GetPostByIdResponse? postResponse = await response.Content.ReadFromJsonAsync<GetPostByIdResponse>();
                if (postResponse?.Success == true)
                {
                    _post = postResponse.Post;
                    _isLoadingPost = false;
                    StateHasChanged();
                    _ = LoadAttachmentsAsync();
                }
                else
                {
                    string errorResponseMessage = await HandleResponse(postResponse);
                    _errorMessage = $"{_errorLoadingPost} {errorResponseMessage}";
                    await HandleInvalidResponse(_errorMessage);
                    _post = null;
                    _isLoadingPost = false;
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

                string errorResponseMessage = await HandleResponse(errorResponse);
                _errorMessage = $"{_errorLoadingPost} {errorResponseMessage}";
                await HandleInvalidResponse(_errorMessage);
                _post = null;
                _isLoadingPost = false;
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"{_errorLoadingPost} {ex.Message}";
            await HandleInvalidResponse(_errorMessage);
            _post = null;
            _isLoadingPost = false;
        }
        finally
        {
            if (_isLoadingPost)
            {
                _isLoadingPost = false;
            }

            StateHasChanged();
        }
    }

    private async Task LoadAttachmentsAsync()
    {
        if (Id == Guid.Empty || _post == null)
        {
            return;
        }

        _isLoadingAttachments = true;
        _attachmentsData = null;
        StateHasChanged();

        try
        {
            string attachmentsApiUrl = $"{Configuration.ApiUrl}/Post/get-attachments-by-id/{Id}";
            HttpResponseMessage attachmentsHttpResponse =
                await HttpClientService.SendRequestAsync(() => Http.GetAsync(attachmentsApiUrl));

            if (attachmentsHttpResponse.IsSuccessStatusCode)
            {
                GetAttachmentsByIdResponse? attachmentsApiResponse =
                    await attachmentsHttpResponse.Content.ReadFromJsonAsync<GetAttachmentsByIdResponse>();
                if (attachmentsApiResponse?.Success == true &&
                    attachmentsApiResponse.Attachments.Any())
                {
                    _attachmentsData = attachmentsApiResponse.Attachments.ToList();
                }
                else
                {
                    _attachmentsData = [];
                }
            }
            else if (attachmentsHttpResponse.StatusCode == HttpStatusCode.NotFound)
            {
                _attachmentsData = [];
            }
            else
            {
                _attachmentsData = [];
            }
        }
        catch (Exception ex)
        {
            _attachmentsData = []; 
        }
        finally
        {
            _isLoadingAttachments = false;
            StateHasChanged();
        }
    }

    private void NavigateToPosts()
    {
        NavigationManager.NavigateTo("/posts");
    }

    private void ShowAttachmentPreview(string attachmentUrl)
    {
        _selectedAttachmentPreview = attachmentUrl;
        StateHasChanged();
    }

    private void CloseAttachmentPreview()
    {
        _selectedAttachmentPreview = null;
        StateHasChanged();
    }

    private string GetCategoryColor(string? categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName) || !_categoryColors.TryGetValue(categoryName, out string? color))
        {
            return string.IsNullOrWhiteSpace(categoryName)
                ? DefaultCategoryColor
                : GetDeterministicColorFromString(categoryName);
        }

        return color;
    }

    private string GetDeterministicColorFromString(string input)
    {
        int hash = input.Aggregate(0, (current, c) => c + ((current << 5) - current));
        int r = Math.Min(200, Math.Max(80, (hash & 0xFF0000) >> 16));
        int g = Math.Min(200, Math.Max(80, (hash & 0x00FF00) >> 8));
        int b = Math.Min(200, Math.Max(80, hash & 0x0000FF));
        return $"#{r:X2}{g:X2}{b:X2}";
    }

    public Task<string> HandleResponse(BaseResponse? response)
    {
        if (response is null)
        {
            return Task.FromResult(_errorResponseNull);
        }

        StringBuilder errorMessageBuilder = new StringBuilder();
        errorMessageBuilder.AppendLine($"{_errorMessagePrefix} {response.Message}");
        errorMessageBuilder.AppendLine($"{_errorStatusCodePrefix} {response.StatusCode}");

        if (response.Errors.Any())
        {
            errorMessageBuilder.AppendLine($"{_errorErrorsPrefix} {string.Join(", ", response.Errors)}");
        }

        return Task.FromResult(errorMessageBuilder.ToString());
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