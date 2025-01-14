using Application.Common.Interfaces;
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
        // services.AddScoped<ICsvExchangeRateService, CsvExchangeRateService>();
        // services.AddScoped<IMlModelService, MlModelService>();
        services.AddScoped<IRateService, RateService>();

        services.AddScoped<IKafkaProducer, KafkaProducer>();
    }
}