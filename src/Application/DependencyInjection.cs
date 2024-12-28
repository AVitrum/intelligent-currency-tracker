using Application.Common.Interfaces;
using Application.ExchangeRates;
using Application.Kafka;
using Application.MlModel;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        services.AddHttpClient();
        
        services.AddScoped<IExchangeRateService, ExchangeRateService>();
        services.AddScoped<ICsvExchangeRateService, CsvExchangeRateService>();
        services.AddScoped<IMlModelService, MlModelService>();
        
        services.AddScoped<IKafkaProducer, KafkaProducer>();

        services.AddScoped<ICsvHelper, Common.Helpers.CsvHelper>();
        
        return services;
    }
}