using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Utils;
using Domain.Events;
using Infrastructure.Identity;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundServices;

public class AlertSenderService : IHostedService
{
    private readonly ILogger<AlertSenderService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ExchangeRateSyncService _syncService;

    public AlertSenderService(
        ILogger<AlertSenderService> logger,
        IServiceScopeFactory scopeFactory,
        ExchangeRateSyncService syncService)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _syncService = syncService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("AlertSenderService is starting.");
        _syncService.AlertSender += OnAlertSenderAsync;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("AlertSenderService is stopping.");
        _syncService.AlertSender -= OnAlertSenderAsync;
        return Task.CompletedTask;
    }

    private async Task OnAlertSenderAsync(object sender, AlertSenderEventArgs e)
    {
        _logger.LogInformation(
            "Received alert event for currency {CurrencyId} with percent difference {PercentDifference}.",
            e.CurrencyId, e.PercentDifference);

        List<UserSettings> userSettings = await GetUserSettingsToNotifyAsync(e.PercentDifference);

        if (userSettings.Count == 0)
        {
            _logger.LogInformation("No users to notify for percent difference {PercentDifference}.",
                e.PercentDifference);
            return;
        }

        IEnumerable<Task> notificationTasks = userSettings.Select(settings => NotifyUserAsync(settings, e));
        await Task.WhenAll(notificationTasks);
    }

    private async Task<List<UserSettings>> GetUserSettingsToNotifyAsync(decimal percentDifference)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IUserSettingsRepository userSettingsRepository =
            scope.ServiceProvider.GetRequiredService<IUserSettingsRepository>();
        IEnumerable<UserSettings> settings =
            await userSettingsRepository.GetUserSettingsRangeByPercentageToNotifyAsync(percentDifference);
        return settings.ToList();
    }

    private async Task NotifyUserAsync(UserSettings settings, AlertSenderEventArgs e)
    {
        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            IEmailSender emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
            ICurrencyRepository currencyRepository = scope.ServiceProvider.GetRequiredService<ICurrencyRepository>();
            IRateRepository rateRepository = scope.ServiceProvider.GetRequiredService<IRateRepository>();
            ITraceableCurrencyRepository traceableCurrencyRepository =
                scope.ServiceProvider.GetRequiredService<ITraceableCurrencyRepository>();
            UserManager<ApplicationUser> userManager =
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            bool isTracked = await traceableCurrencyRepository.ExistsAsync(settings.UserId, e.CurrencyId);
            if (!isTracked)
            {
                _logger.LogInformation("Currency {CurrencyId} is not tracked by user {UserId}. Skipping.", e.CurrencyId,
                    settings.UserId);
                return;
            }

            Currency currency = await currencyRepository.GetByIdAsync(e.CurrencyId);
            ApplicationUser? user = await userManager.FindByIdAsync(settings.UserId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found.", settings.UserId);
                return;
            }

            Rate lastRate = await rateRepository.GetLastByCurrencyIdAsync(e.CurrencyId);
            Rate previousRate = await rateRepository.GetPreviousByCurrencyIdAsync(e.CurrencyId, lastRate.Date);

            decimal percentDifference = previousRate.Value != 0
                ? (lastRate.Value - previousRate.Value) / previousRate.Value * 100m
                : 0m;

            string emailBody = BuildEmailBody(currency, lastRate, previousRate, percentDifference);

            await SendEmailAsync(emailSender, user.Email!, currency, percentDifference, emailBody);

            _logger.LogInformation("Alert email sent to user {UserId} ({Email}) for currency {CurrencyCode}.",
                user.Id, user.Email, currency.Code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying user {UserId} for currency {CurrencyId}.", settings.UserId,
                e.CurrencyId);
        }
    }

    private string BuildEmailBody(Currency currency, Rate lastRate, Rate previousRate, decimal percentDifference)
    {
        string percentString = $"{percentDifference:0.##}%";
        string lastRateValue = lastRate.Value.ToString("0.####");
        string previousRateValue = previousRate.Value.ToString("0.####");

        return $"""
                <html>
                  <body style='font-family: Arial, sans-serif; background: #f9f9f9; padding: 20px;'>
                    <div style='max-width: 500px; margin: auto; background: #fff; border-radius: 8px; box-shadow: 0 2px 8px #eee; padding: 24px;'>
                      <h2 style='color: #2d7ff9;'>Currency Alert</h2>
                      <p>
                        The currency <b>{currency.Name}</b> has changed by <b style='color: #e74c3c;'>{percentString}</b>.
                      </p>
                      <table style='width: 100%; border-collapse: collapse; margin-top: 16px;'>
                        <tr>
                          <th style='text-align: left; padding: 8px; border-bottom: 1px solid #eee;'>Currency</th>
                          <td style='padding: 8px; border-bottom: 1px solid #eee;'>{currency.Name}</td>
                        </tr>
                        <tr>
                          <th style='text-align: left; padding: 8px; border-bottom: 1px solid #eee;'>Change</th>
                          <td style='padding: 8px; border-bottom: 1px solid #eee;'>{percentString}</td>
                        </tr>
                        <tr>
                          <th style='text-align: left; padding: 8px; border-bottom: 1px solid #eee;'>Last Rate</th>
                          <td style='padding: 8px; border-bottom: 1px solid #eee;'>{lastRateValue}</td>
                        </tr>
                        <tr>
                          <th style='text-align: left; padding: 8px; border-bottom: 1px solid #eee;'>Previous Rate</th>
                          <td style='padding: 8px; border-bottom: 1px solid #eee;'>{previousRateValue}</td>
                        </tr>
                        <tr>
                          <th style='text-align: left; padding: 8px;'>Date</th>
                          <td style='padding: 8px;'>{lastRate.Date:yyyy-MM-dd}</td>
                        </tr>
                      </table>
                      <p style='margin-top: 24px; color: #888; font-size: 13px;'>This is an automated alert from your currency tracker.</p>
                    </div>
                  </body>
                </html>
                """;
    }

    private async Task SendEmailAsync(
        IEmailSender emailSender,
        string email,
        Currency currency,
        decimal percentDifference,
        string htmlBody)
    {
        string subject = $"Currency {currency.Code} alert: percentage difference is {percentDifference:0.##}%";
        await emailSender.SendEmailAsync(email, subject, htmlBody);
    }
}