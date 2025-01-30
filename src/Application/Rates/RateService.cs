using Application.Common.Interfaces;
using Application.Rates.Results;
using Domain.Common;
using Shared.Payload.Requests;

namespace Application.Rates;

public class RateService : IRateService
{
    private readonly IRateHelper _rateHelper;

    public RateService(IRateHelper rateHelper)
    {
        _rateHelper = rateHelper;
    }

    public async Task<BaseResult> GetRatesAsync(ExchangeRateRequest request)
    {
        var ratesDto = _rateHelper.ConvertRatesToDtoAsync(
            await _rateHelper.GetRatesFromRequestAsync(request));

        return GetRatesResult.SuccessResult(ratesDto);
    }
}