using Application.Common.Models;
using Domain.Common;

namespace Application.ExchangeRates.Results;

public class GetExchangeRateRangeResult : BaseResult
{
    public IEnumerable<ExchangeRateDto> Data { get; }
    
    private GetExchangeRateRangeResult(bool success, IEnumerable<string> errors, IEnumerable<ExchangeRateDto> data) : base(success, errors)
    {
        Data = data;
    }

    public static GetExchangeRateRangeResult SuccessResult(IEnumerable<ExchangeRateDto> exchangeRates) =>
        new(true, [], exchangeRates);
}