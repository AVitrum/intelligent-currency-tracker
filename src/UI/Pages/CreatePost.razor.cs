using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Blazored.Toast.Services;
using Domain.Common;
using Domain.Enums;
using Shared.Payload.Responses;
using Shared.Payload.Responses.Identity;
using UI.Common.Interfaces;
using UI.Services;
using IConfiguration = UI.Common.Interfaces.IConfiguration;

namespace UI.Pages;

public partial class CreatePost : ComponentBase, IPageComponent, IAsyncDisposable
{
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private IConfiguration Configuration { get; set; } = null!;
    [Inject] private IHttpClientService HttpClientService { get; set; } = null!;
    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private LocalizationService Localizer { get; set; } = null!;
    [Inject] private UserSettingsService UserSettingsService { get; set; } = null!;

    private CreatePostFormModel PostModel { get; } = new CreatePostFormModel();
    private List<IBrowserFile> SelectedFiles { get; } = [];
    private bool IsSubmitting { get; set; }
    private readonly List<PostCategory> _postCategories = Enum.GetValues<PostCategory>().ToList();
    private readonly List<Language> _languages = Enum.GetValues<Language>().ToList();

    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    private readonly string[] _allowedExtensions =
        [".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".txt"];

    private string _pageTitle = "";
    private string _headerTitle = "";
    private string _headerDescription = "";
    private string _formCardTitle = "";
    private string _labelTitle = "";
    private string _placeholderTitle = "";
    private string _labelContent = "";
    private string _placeholderContent = "";
    private string _labelCategory = "";
    private string _optionSelectCategory = "";
    private string _labelLanguage = "";
    private string _optionSelectLanguage = "";
    private string _labelAttachments = "";
    private string _buttonCreatePost = "";
    private string _buttonCreating = "";
    private string _toastPostCreatedSuccessfully = "";
    private string _toastFileTooLargeFormat = "";
    private string _toastInvalidFileExtensionFormat = "";
    private string _errorGeneric = "";
    private string _errorMessagePrefix = "";
    private string _errorStatusCodePrefix = "";
    private string _errorErrorsPrefix = "";
    private string _errorResponseNull = "";

    public class CreatePostFormModel
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required.")]
        [StringLength(5000, ErrorMessage = "Content cannot be longer than 5000 characters.")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required.")]
        public string Category { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Language is required.")]
        public string Language { get; set; } = string.Empty;
    }

    protected override async Task OnInitializedAsync()
    {
        if (await GetUserRoleAsync() != nameof(UserRole.PUBLISHER))
        {
            ToastService.ShowError("You do not have permission to create a post.");
            NavigationManager.NavigateTo("/");
        }
        
        await LoadLocalizedStringsAsync();
        UserSettingsService.OnSettingsChangedAsync += HandleSettingsChangedAsync;
        if (string.IsNullOrEmpty(PostModel.Category) && _postCategories.Any())
        {
            PostModel.Category = _postCategories.First().ToString();
        }
    }

    private async Task LoadLocalizedStringsAsync()
    {
        _pageTitle = await Localizer.GetStringAsync("createpost.page_title");
        _headerTitle = await Localizer.GetStringAsync("createpost.header.title");
        _headerDescription = await Localizer.GetStringAsync("createpost.header.description");
        _formCardTitle = await Localizer.GetStringAsync("createpost.form.card_title");
        _labelTitle = await Localizer.GetStringAsync("createpost.form.label.title");
        _placeholderTitle = await Localizer.GetStringAsync("createpost.form.placeholder.title");
        _labelContent = await Localizer.GetStringAsync("createpost.form.label.content");
        _placeholderContent = await Localizer.GetStringAsync("createpost.form.placeholder.content");
        _labelCategory = await Localizer.GetStringAsync("createpost.form.label.category");
        _optionSelectCategory = await Localizer.GetStringAsync("createpost.form.option.select_category");
        _labelLanguage = await Localizer.GetStringAsync("createpost.form.label.language");
        _optionSelectLanguage = await Localizer.GetStringAsync("createpost.form.option.select_language");
        _labelAttachments = await Localizer.GetStringAsync("createpost.form.label.attachments");
        _buttonCreatePost = await Localizer.GetStringAsync("createpost.form.button.create_post");
        _buttonCreating = await Localizer.GetStringAsync("createpost.form.button.creating");
        _toastPostCreatedSuccessfully = await Localizer.GetStringAsync("createpost.toast.post_created_successfully");
        _toastFileTooLargeFormat = await Localizer.GetStringAsync("contacts.toast.file_too_large_format");
        _toastInvalidFileExtensionFormat =
            await Localizer.GetStringAsync("contacts.toast.invalid_file_extension_format");
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

    private void LoadFiles(InputFileChangeEventArgs e)
    {
        SelectedFiles.Clear();
        foreach (IBrowserFile file in e.GetMultipleFiles())
        {
            if (file.Size > MaxFileSize)
            {
                ToastService.ShowError(string.Format(_toastFileTooLargeFormat, file.Name, FormatBytes(MaxFileSize)));
                continue;
            }

            string extension = Path.GetExtension(file.Name).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                ToastService.ShowError(string.Format(_toastInvalidFileExtensionFormat, file.Name,
                    string.Join(", ", _allowedExtensions)));
                continue;
            }

            SelectedFiles.Add(file);
        }
    }

    private string FormatBytes(long bytes, int decimals = 2)
    {
        if (bytes == 0)
        {
            return "0 Bytes";
        }

        const int k = 1024;
        string[] sizes = ["Bytes", "KB", "MB", "GB", "TB"];
        int i = Convert.ToInt32(Math.Floor(Math.Log(bytes) / Math.Log(k)));
        return $"{Math.Round(bytes / Math.Pow(k, i), decimals)} {sizes[i]}";
    }

    private async Task<string> GetUserRoleAsync()
    {
        try
        {
            string url = $"{Configuration.ApiUrl}/Identity/get-user-role";
            HttpResponseMessage resp = await HttpClientService.SendRequestAsync(() => Http.GetAsync(url));
            UserRoleResponse? response = await resp.Content.ReadFromJsonAsync<UserRoleResponse>();


            if (resp.IsSuccessStatusCode && response?.Role != null)
            {
                return response.Role;
            }

            string errorMessage = await HandleResponse(response);
            await HandleInvalidResponse(errorMessage);
            return string.Empty;
        }
        catch (Exception e)
        {
            await HandleInvalidResponse($"{_errorGeneric} {e.Message}");
            return string.Empty;
        }
    }

    private async Task HandleCreatePostAsync()
    {
        IsSubmitting = true;
        StateHasChanged();

        try
        {
            using MultipartFormDataContent content = new MultipartFormDataContent();
            content.Add(new StringContent(PostModel.Title), "title");
            content.Add(new StringContent(PostModel.Content), "content");
            content.Add(new StringContent(PostModel.Category), "category");

            foreach (IBrowserFile file in SelectedFiles)
            {
                StreamContent fileContent = new StreamContent(file.OpenReadStream(MaxFileSize));
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(fileContent, "attachments", file.Name);
            }

            string apiUrl = $"{Configuration.ApiUrl}/Post/create";
            HttpResponseMessage response =
                await HttpClientService.SendRequestAsync(() => Http.PostAsync(apiUrl, content));

            if (response.IsSuccessStatusCode)
            {
                ToastService.ShowSuccess(_toastPostCreatedSuccessfully);
                PostModel.Title = string.Empty;
                PostModel.Content = string.Empty;
                if (_postCategories.Count != 0)
                {
                    PostModel.Category = _postCategories.First().ToString();
                }

                SelectedFiles.Clear();
                StateHasChanged();
            }
            else
            {
                DefaultResponse? errorResponse = null;
                try
                {
                    errorResponse = await response.Content.ReadFromJsonAsync<DefaultResponse>();
                }
                catch (Exception)
                {
                    // ignored
                }

                string errorMessage = await HandleResponse(errorResponse);
                await HandleInvalidResponse(errorMessage);
            }
        }
        catch (Exception ex)
        {
            await HandleInvalidResponse($"{_errorGeneric} {ex.Message}");
        }
        finally
        {
            IsSubmitting = false;
            StateHasChanged();
        }
    }

    public async ValueTask DisposeAsync()
    {
        UserSettingsService.OnSettingsChangedAsync -= HandleSettingsChangedAsync;
        await Task.CompletedTask;
        GC.SuppressFinalize(this);
    }
}