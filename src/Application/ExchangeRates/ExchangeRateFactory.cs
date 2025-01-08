using Application.Common.Interfaces;
using Domain.Entities;
using Newtonsoft.Json.Linq;

namespace Application.ExchangeRates;

public class ExchangeRateFactory : IExchangeRateFactory
{
    public ExchangeRate CreateExchangeRate(JToken rateToken, DateTime date)
    {
        var rate = new ExchangeRate
        {
            Date = date,
            Currency = rateToken["currency"]?.ToString() ?? throw new Exception("Currency is missing"),
            SaleRateNb = rateToken["saleRateNB"]?.ToObject<decimal>() ?? 0,
            PurchaseRateNb = rateToken["purchaseRateNB"]?.ToObject<decimal>() ?? 0,
            SaleRate = rateToken["saleRate"]?.ToObject<decimal>() ?? 0,
            PurchaseRate = rateToken["purchaseRate"]?.ToObject<decimal>() ?? 0
        };
        return rate;
    }

    public ExchangeRate CreateExchangeRate(ExchangeRate data)
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
        return rate;
    }

    public async Task<ExchangeRate> CreateExchangeRateFromDelegate(ExchangeRateDelegate rateDelegate, PostCreationDelegate? postCreationDelegate = null)
    {
        ExchangeRate rate = rateDelegate() ?? throw new Exception("Exchange rate is null");
        if (postCreationDelegate is not null)
        {
            await postCreationDelegate(rate);
        }
        return rate;
    }
}