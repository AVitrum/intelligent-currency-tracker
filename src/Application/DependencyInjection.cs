using Application.AiModel;
using Application.Common.Helpers;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Utils;
using Application.Kafka;
using Application.Rates;
using Application.Reports;
using Microsoft.Extensions.DependencyInjection;
using Shared;

namespace Application;

public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddShared();

        services.AddScoped<IRateHelper, RateHelper>();
        services.AddScoped<IMinioHelper, MinioHelper>();

        services.AddScoped<ICsvService, CsvService>();
        services.AddScoped<IRateService, RateService>();
        services.AddScoped<IAiModelService, AiModelService>();
        services.AddScoped<IReportService, ReportService>();

        services.AddScoped<IKafkaProducer, KafkaProducer>();
    }
}