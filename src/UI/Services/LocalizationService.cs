using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.JSInterop;
using Shared.Dtos;


namespace UI.Services;

public class LocalizationService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly UserSettingsService _userSettingsService;
    private readonly IJSRuntime _jsRuntime;
    private readonly string _defaultLang = "en";
    private readonly string _basePath = "localization";

    public LocalizationService(
        HttpClient httpClient,
        IMemoryCache memoryCache,
        UserSettingsService userSettingsService,
        IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _cache = memoryCache;
        _userSettingsService = userSettingsService;
        _jsRuntime = jsRuntime;
    }

    private async Task<Dictionary<string, string>?> GetTranslationsAsync(string lang)
    {
        if (string.IsNullOrWhiteSpace(lang))
        {
            lang = _defaultLang;
        }

        string cacheKey = $"translations_{lang}";

        if (_cache.TryGetValue(cacheKey, out Dictionary<string, string>? translations))
        {
            return translations;
        }

        string filePath = $"{_basePath}/{lang}.json";
        try
        {
            translations = await _httpClient.GetFromJsonAsync<Dictionary<string, string>>(filePath);
            if (translations != null)
            {
                MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromHours(1));
                _cache.Set(cacheKey, translations, cacheEntryOptions);
                return translations;
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error loading translation file '{filePath}': {ex.Message}");

            if (lang != _defaultLang)
            {
                Console.WriteLine($"Falling back to default language: '{_defaultLang}'");
                return await GetTranslationsAsync(_defaultLang);
            }
        }

        return new Dictionary<string, string>();
    }

    public async Task<string> GetStringAsync(string key)
    {
        SettingsDto settings = await _userSettingsService.GetSettingsAsync(_jsRuntime);
        string currentLang = settings.Language;

        if (string.IsNullOrWhiteSpace(currentLang))
        {
            currentLang = _defaultLang;
        }

        Dictionary<string, string>? translations = await GetTranslationsAsync(currentLang);
        if (translations != null && translations.TryGetValue(key, out string? value))
        {
            return value;
        }

        if (currentLang != _defaultLang)
        {
            Dictionary<string, string>? defaultTranslations = await GetTranslationsAsync(_defaultLang);
            if (defaultTranslations != null && defaultTranslations.TryGetValue(key, out string? defaultValue))
            {
                return defaultValue;
            }
        }

        return $"[{key}]";
    }
}