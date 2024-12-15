using Application.Common.Models;
using Domain.Common;

namespace Application.ExchangeRates.Results;

public class GetExchangeRateRangeResult : BaseResult
{
    public GetExchangeRateListDto Data { get; }
    
    private GetExchangeRateRangeResult(bool success, IEnumerable<string> errors, GetExchangeRateListDto data) : base(success, errors)
    {
        Data = data;
    }

    public static GetExchangeRateRangeResult SuccessResult(GetExchangeRateListDto data) =>
        new(true, [], data);
}