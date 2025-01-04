using Application.ExchangeRates;
using Domain.Entities;
using Newtonsoft.Json.Linq;

namespace Application.Common.Interfaces
{
    public interface IExchangeRateFactory
    {
        Task<ExchangeRate> CreateExchangeRate(JToken rateToken);
        Task<ExchangeRate> CreateExchangeRate(ExchangeRate data);
        Task<ExchangeRate> CreateExchangeRateFromDelegate(ExchangeRateDelegate rateDelegate, PostCreationDelegate? postCreationDelegate = null);
    }
}