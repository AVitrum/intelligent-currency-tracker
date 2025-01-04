using Application.Common.Interfaces;
using Domain.Entities;
using Newtonsoft.Json.Linq;

namespace Application.ExchangeRates;

public class ExchangeRateFactory : IExchangeRateFactory
{
    private readonly IExchangeRateRepository _repository;

    public ExchangeRateFactory(IExchangeRateRepository repository)
    {
        _repository = repository;
    }

    public async Task<ExchangeRate> CreateExchangeRate(JToken rateToken)
    {
        var rate = new ExchangeRate
        {
            Date = rateToken["date"]?.ToObject<DateTime>() ?? DateTime.UtcNow,
            Currency = rateToken["currency"]?.ToString() ?? throw new Exception("Currency is missing"),
            SaleRateNb = rateToken["saleRateNB"]?.ToObject<decimal>() ?? 0,
            PurchaseRateNb = rateToken["purchaseRateNB"]?.ToObject<decimal>() ?? 0,
            SaleRate = rateToken["saleRate"]?.ToObject<decimal>() ?? 0,
            PurchaseRate = rateToken["purchaseRate"]?.ToObject<decimal>() ?? 0
        };
        await _repository.AddAsync(rate);
        return rate;
    }

    public async Task<ExchangeRate> CreateExchangeRate(ExchangeRate data)
    {
        var rate = new ExchangeRate
        {
            Date = data.Date,
            Currency = data.Currency,
            SaleRateNb = data.SaleRateNb,
            PurchaseRateNb = data.PurchaseRateNb,
            SaleRate = data.SaleRate,
            PurchaseRate = data.PurchaseRate
        };
        await _repository.AddAsync(rate);
        return rate;
    }

    public async Task<ExchangeRate> CreateExchangeRateFromDelegate(ExchangeRateDelegate rateDelegate, PostCreationDelegate? postCreationDelegate = null)
    {
        ExchangeRate rate = rateDelegate() ?? throw new Exception("Exchange rate is null");
        await _repository.AddAsync(rate);
        
        if (postCreationDelegate is not null)
        {
            await postCreationDelegate(rate);
        }
        return rate;
    }
}