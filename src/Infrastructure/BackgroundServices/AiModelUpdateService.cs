using Application.AiModel.Results;
using Application.Common.Interfaces.Services;
using Domain.Common;
using Domain.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundServices;

public class AiModelUpdateService : IHostedService
{
    private readonly ExchangeRateSyncService _syncService;
    private readonly ILogger<AiModelUpdateService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public AiModelUpdateService(
        ExchangeRateSyncService syncService,
        ILogger<AiModelUpdateService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _syncService = syncService;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _syncService.ExchangeRatesFetched += OnFetchedAsync;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _syncService.ExchangeRatesFetched -= OnFetchedAsync;
        return Task.CompletedTask;
    }

    private async Task OnFetchedAsync(object sender, ExchangeRatesFetchedEventArgs e)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IAiModelService aiModelService = scope.ServiceProvider.GetRequiredService<IAiModelService>();

        foreach (int r030 in e.R030)
        {
            await TryTrainWithRetryAsync(aiModelService, r030);
        }
    }

    private async Task TryTrainWithRetryAsync(IAiModelService aiModelService, int r030)
    {
        while (true)
        {
            BaseResult result = await aiModelService.TrainModelAsync(r030);

            if (result is TrainResult trainResult)
            {
                _logger.LogInformation("AI training started: {Message}", trainResult.Message);
            }
            else
            {
                _logger.LogError("AI training failed for {R030}, retrying in 15 minutes. Error: {Error}", r030, result);
                await Task.Delay(TimeSpan.FromMinutes(15));
                continue;
            }
            
            break;
        }
    }
}