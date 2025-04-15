using Application.AiModel;
using Application.Common.Helpers;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Utils;
using Application.ExchangeRates;
using Application.Kafka;
using Application.Rates;
using Microsoft.Extensions.DependencyInjection;
using Shared;

namespace Application;

public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddShared();

        services.AddScoped<ICsvService, CsvService>();
        services.AddScoped<IRateService, RateService>();
        services.AddScoped<IRateHelper, RateHelper>();
        services.AddScoped<IAiModelService, AiModelService>();

        services.AddScoped<IKafkaProducer, KafkaProducer>();
    }
}