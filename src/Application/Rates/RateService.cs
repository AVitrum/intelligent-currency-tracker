using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Utils;
using Application.Rates.Results;
using Domain.Common;
using Shared.Dtos;
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
        IEnumerable<RateDto> ratesDto = _rateHelper.ConvertRatesToDtoAsync(
            await _rateHelper.GetRatesFromRequestAsync(request));

        return GetRatesResult.SuccessResult(ratesDto);
    }
}