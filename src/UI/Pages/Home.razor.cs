using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.Helpers;
using UI.Services; 

namespace UI.Pages;

public partial class Home : ComponentBase, IAsyncDisposable
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;
    
    [Inject]
    private IJSRuntime Js { get; set; } = null!;

    [Inject]
    private LocalizationService Localizer { get; set; } = null!;
    
    [Inject]
    private UserSettingsService UserSettingsService { get; set; } = null!;

    private string _pageTitle = "";
    private string _welcomeTitle = "";
    private string _welcomeDescription = "";
    private string _heroBannerAlt = "";
    private string _keyFeaturesTitle = "";
    private string _featureRealTimeTrackingTitle = "";
    private string _featureRealTimeTrackingDescription = "";
    private string _featureHistoricalAnalysisTitle = "";
    private string _featureHistoricalAnalysisDescription = "";
    private string _featureAiForecastsTitle = "";
    private string _featureAiForecastsDescription = "";
    private string _featureCustomDashboardTitle = "";
    private string _featureCustomDashboardDescription = "";
    private string _howItWorksTitle = "";
    private string _step1Alt = "";
    private string _step1Title = "";
    private string _step1Description = "";
    private string _step2Alt = "";
    private string _step2Title = "";
    private string _step2Description = "";
    private string _step3Alt = "";
    private string _step3Title = "";
    private string _step3Description = "";
    private string _ctaTitle = "";
    private string _ctaDescription = "";
    private string _ctaButtonDashboard = "";
    private string _ctaButtonSignup = "";
    private string _footerCopyrightPrefix = "";
    private string _footerCopyrightSuffix = "";
    private string _footerLinkPrivacy = "";
    private string _footerLinkTerms = "";
    private string _footerLinkContact = "";


    protected override async Task OnInitializedAsync()
    {
        await LoadLocalizedStringsAsync();
        UserSettingsService.OnSettingsChangedAsync += async () => await LoadLocalizedStringsAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadLocalizedStringsAsync()
    {
        _pageTitle = await Localizer.GetStringAsync("home.page_title");
        _welcomeTitle = await Localizer.GetStringAsync("home.welcome_title");
        _welcomeDescription = await Localizer.GetStringAsync("home.welcome_description");
        _heroBannerAlt = await Localizer.GetStringAsync("home.hero_banner_alt");
        _keyFeaturesTitle = await Localizer.GetStringAsync("home.key_features_title");
        _featureRealTimeTrackingTitle = await Localizer.GetStringAsync("home.feature.real_time_tracking.title");
        _featureRealTimeTrackingDescription = await Localizer.GetStringAsync("home.feature.real_time_tracking.description");
        _featureHistoricalAnalysisTitle = await Localizer.GetStringAsync("home.feature.historical_analysis.title");
        _featureHistoricalAnalysisDescription = await Localizer.GetStringAsync("home.feature.historical_analysis.description");
        _featureAiForecastsTitle = await Localizer.GetStringAsync("home.feature.ai_forecasts.title");
        _featureAiForecastsDescription = await Localizer.GetStringAsync("home.feature.ai_forecasts.description");
        _featureCustomDashboardTitle = await Localizer.GetStringAsync("home.feature.custom_dashboard.title");
        _featureCustomDashboardDescription = await Localizer.GetStringAsync("home.feature.custom_dashboard.description");
        _howItWorksTitle = await Localizer.GetStringAsync("home.how_it_works_title");
        _step1Alt = await Localizer.GetStringAsync("home.step1_alt");
        _step1Title = await Localizer.GetStringAsync("home.step1.title");
        _step1Description = await Localizer.GetStringAsync("home.step1.description");
        _step2Alt = await Localizer.GetStringAsync("home.step2_alt");
        _step2Title = await Localizer.GetStringAsync("home.step2.title");
        _step2Description = await Localizer.GetStringAsync("home.step2.description");
        _step3Alt = await Localizer.GetStringAsync("home.step3_alt");
        _step3Title = await Localizer.GetStringAsync("home.step3.title");
        _step3Description = await Localizer.GetStringAsync("home.step3.description");
        _ctaTitle = await Localizer.GetStringAsync("home.cta.title");
        _ctaDescription = await Localizer.GetStringAsync("home.cta.description");
        _ctaButtonDashboard = await Localizer.GetStringAsync("home.cta.button.dashboard");
        _ctaButtonSignup = await Localizer.GetStringAsync("home.cta.button.signup");
        _footerCopyrightPrefix = await Localizer.GetStringAsync("home.footer.copyright_text_prefix");
        _footerCopyrightSuffix = await Localizer.GetStringAsync("home.footer.copyright_text_suffix");
        _footerLinkPrivacy = await Localizer.GetStringAsync("home.footer.link.privacy");
        _footerLinkTerms = await Localizer.GetStringAsync("home.footer.link.terms");
        _footerLinkContact = await Localizer.GetStringAsync("home.footer.link.contact");
        StateHasChanged(); 
    }

    private void NavigateToDashboard()
    {
        NavigationManager.NavigateTo("/dashboard");
    }

    private void NavigateToRegister()
    {
        NavigationManager.NavigateTo("/auth");
    }
    
    private async Task HandleSettingsChangedAsync()
    {
        await LoadLocalizedStringsAsync();
        StateHasChanged();
    }
    
    public async ValueTask DisposeAsync()
    {
        UserSettingsService.OnSettingsChangedAsync -= HandleSettingsChangedAsync;
        await Task.CompletedTask;
        GC.SuppressFinalize(this);
    }
}