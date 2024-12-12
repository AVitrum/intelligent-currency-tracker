using Application.Books;
using Application.Common.Interfaces;
using Application.ExchangeRates;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        services.AddHttpClient();
        
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IExchangeRateService, ExchangeRateService>();
        return services;
    }
}