using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.ExchangeRates;
using Application.Kafka;
using Application.Rates;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        services.AddHttpClient();
        // services.AddScoped<IMlModelService, MlModelService>();
        services.AddScoped<ICsvService, CsvService>();
        services.AddScoped<IRateService, RateService>();
        services.AddScoped<IRateHelper, RateHelper>();

        services.AddScoped<IKafkaProducer, KafkaProducer>();
    }
}