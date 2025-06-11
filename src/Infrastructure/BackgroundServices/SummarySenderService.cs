using System.Globalization;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Utils;
using Domain.Common;
using Domain.Enums;
using Infrastructure.Identity;
using Infrastructure.Identity.SubUserEntities;
using Infrastructure.Interfaces;
using Infrastructure.Summary.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Dtos;
using Shared.Payload.Requests;

namespace Infrastructure.BackgroundServices;

public class SummarySenderService : BackgroundService
{
    private readonly ILogger<SummarySenderService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public SummarySenderService(IServiceScopeFactory scopeFactory, ILogger<SummarySenderService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SummarySenderService started.");
        using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        DateTime lastDaily = DateTime.MinValue, lastWeekly = DateTime.MinValue, lastMonthly = DateTime.MinValue;

        do
        {
            try
            {
                DateTime now = DateTime.UtcNow;

                // Daily: at 00:01 UTC
                if (now is { Hour: 0, Minute: 1 } && lastDaily.Date != now.Date)
                {
                    _logger.LogInformation("Triggering daily summary at {Time}.", now);
                    await RunJobAsync(SummaryType.Daily);
                    lastDaily = now.Date;
                }

                // Weekly: Sunday 23:59 UTC
                CultureInfo culture = CultureInfo.InvariantCulture;
                Calendar calendar = culture.Calendar;
                DayOfWeek dayOfWeek = calendar.GetDayOfWeek(now);
                if (dayOfWeek == DayOfWeek.Sunday && now is { Hour: 23, Minute: 59 } &&
                    lastWeekly.Date != now.Date)
                {
                    _logger.LogInformation("Triggering weekly summary at {Time}.", now);
                    await RunJobAsync(SummaryType.Weekly);
                    lastWeekly = now.Date;
                }

                // Monthly: last day of month, 23:59 UTC
                int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
                if (now.Day == daysInMonth && now is { Hour: 23, Minute: 59 } &&
                    lastMonthly.Month != now.Month)
                {
                    _logger.LogInformation("Triggering monthly summary at {Time}.", now);
                    await RunJobAsync(SummaryType.Monthly);
                    lastMonthly = now.Date;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating summary");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));

        _logger.LogInformation("SummarySenderService stopped.");
    }

    private async Task RunJobAsync(SummaryType summaryType)
    {
        _logger.LogInformation("RunJobAsync started for {SummaryType}.", summaryType);
        List<UserSettings> userSettings;
        using (IServiceScope scope = _scopeFactory.CreateScope())
        {
            IUserSettingsRepository userSettingsRepository =
                scope.ServiceProvider.GetRequiredService<IUserSettingsRepository>();
            userSettings = (List<UserSettings>)await userSettingsRepository
                .GetUserSettingsRangeBySummaryTypeAsync(summaryType);
        }

        IEnumerable<Task> tasks = userSettings.Select(async settings =>
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            IEmailSender emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
            ISummaryService summaryService = scope.ServiceProvider.GetRequiredService<ISummaryService>();
            ITraceableCurrencyRepository traceableCurrencyRepository =
                scope.ServiceProvider.GetRequiredService<ITraceableCurrencyRepository>();
            ICurrencyRepository currencyRepository = scope.ServiceProvider.GetRequiredService<ICurrencyRepository>();
            UserManager<ApplicationUser> userManager =
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            ApplicationUser? user = await userManager.FindByIdAsync(settings.UserId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", settings.UserId);
                return;
            }

            List<TraceableCurrency> traceableCurrencies = (List<TraceableCurrency>)await traceableCurrencyRepository
                .GetByUserIdAsync(user.Id);

            if (traceableCurrencies.Count == 0)
            {
                _logger.LogInformation("User {UserId} has no traceable currencies.", user.Id);
                return;
            }

            foreach (TraceableCurrency traceableCurrency in traceableCurrencies)
            {
                Currency currency = await currencyRepository.GetByIdAsync(traceableCurrency.CurrencyId);
                ExchangeRateRequest request = CreateExchangeRateRequest(currency.Code, summaryType);

                _logger.LogInformation(
                    "Generating summary for user {UserId}, currency {Currency}, period {Start} - {End}.",
                    user.Id, currency.Code, request.StartDateString, request.EndDateString);

                BaseResult result =
                    await summaryService.GenerateSummaryAsync(request.Start, request.End, request.Currency!);
                if (result is not GenerateSummaryResult generateSummaryResult)
                {
                    _logger.LogWarning("Failed to generate summary for user {UserId}, currency {Currency}.", user.Id,
                        currency.Code);
                    continue;
                }

                SummaryDto summary = generateSummaryResult.Summary;
                string html = GenerateSummaryHtml(summary, request.Start, request.End);

                try
                {
                    await emailSender.SendEmailAsync(
                        user.Email!,
                        $"Report for {request.StartDateString} to {request.EndDateString} ({request.Currency})",
                        html);
                    _logger.LogInformation("Summary email sent to {Email} for currency {Currency}.", user.Email,
                        currency.Code);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email to {Email} for currency {Currency}.", user.Email,
                        currency.Code);
                }
            }
        });

        await Task.WhenAll(tasks);
        _logger.LogInformation("RunJobAsync finished for {SummaryType}.", summaryType);
    }

    private static ExchangeRateRequest CreateExchangeRateRequest(string currencyCode, SummaryType summaryType)
    {
        DateTime end = DateTime.UtcNow;
        DateTime start = summaryType switch
        {
            SummaryType.Daily => end,
            SummaryType.Weekly => end.AddDays(-7),
            _ => end.AddMonths(-1)
        };

        return new ExchangeRateRequest
        {
            Currency = currencyCode,
            StartDateString = start.ToString("dd.MM.yyyy"),
            EndDateString = end.ToString("dd.MM.yyyy")
        };
    }

    private static string GenerateSummaryHtml(SummaryDto summary, DateTime startDate, DateTime endDate)
    {
        return $"""
                    <html>
                        <body style="font-family: Arial, sans-serif; background-color: #f9f9f9; padding: 20px; color: #333;">
                            <h2 style="color: #2c3e50;">Summary Report</h2>
                            <table style="border-collapse: collapse; width: 100%; max-width: 600px; background-color: #ffffff; border: 1px solid #ddd;">
                                <tr style="background-color: #f0f0f0;">
                                    <th style="padding: 10px; text-align: left; border: 1px solid #ddd;">Range</th>
                                    <td style="padding: 10px; border: 1px solid #ddd;">{startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}</td>
                                </tr>
                                <tr>
                                    <th style="padding: 10px; text-align: left; border: 1px solid #ddd;">Average</th>
                                    <td style="padding: 10px; border: 1px solid #ddd;">{summary.Average}</td>
                                </tr>
                                <tr>
                                    <th style="padding: 10px; text-align: left; border: 1px solid #ddd;">Min</th>
                                    <td style="padding: 10px; border: 1px solid #ddd;">{summary.Min}</td>
                                </tr>
                                <tr>
                                    <th style="padding: 10px; text-align: left; border: 1px solid #ddd;">Max</th>
                                    <td style="padding: 10px; border: 1px solid #ddd;">{summary.Max}</td>
                                </tr>
                                <tr>
                                    <th style="padding: 10px; text-align: left; border: 1px solid #ddd;">First</th>
                                    <td style="padding: 10px; border: 1px solid #ddd;">{summary.First}</td>
                                </tr>
                                <tr>
                                    <th style="padding: 10px; text-align: left; border: 1px solid #ddd;">Last</th>
                                    <td style="padding: 10px; border: 1px solid #ddd;">{summary.Last}</td>
                                </tr>
                                <tr>
                                    <th style="padding: 10px; text-align: left; border: 1px solid #ddd;">Absolute Change</th>
                                    <td style="padding: 10px; border: 1px solid #ddd;">{summary.AbsoluteChange}</td>
                                </tr>
                                <tr>
                                    <th style="padding: 10px; text-align: left; border: 1px solid #ddd;">Relative Change (%)</th>
                                    <td style="padding: 10px; border: 1px solid #ddd;">{summary.RelativeChangePercent}</td>
                                </tr>
                                <tr>
                                    <th style="padding: 10px; text-align: left; border: 1px solid #ddd;">Standard Deviation</th>
                                    <td style="padding: 10px; border: 1px solid #ddd;">{summary.StandardDeviation}</td>
                                </tr>
                                <tr>
                                    <th style="padding: 10px; text-align: left; border: 1px solid #ddd;">Coefficient of Variation</th>
                                    <td style="padding: 10px; border: 1px solid #ddd;">{summary.CoefficientOfVariation}</td>
                                </tr>
                                <tr>
                                    <th style="padding: 10px; text-align: left; border: 1px solid #ddd;">Days Increased</th>
                                    <td style="padding: 10px; border: 1px solid #ddd;">{summary.DaysIncreased}</td>
                                </tr>
                                <tr>
                                    <th style="padding: 10px; text-align: left; border: 1px solid #ddd;">Days Decreased</th>
                                    <td style="padding: 10px; border: 1px solid #ddd;">{summary.DaysDecreased}</td>
                                </tr>
                            </table>
                        </body>
                    </html>
                """;
    }
}