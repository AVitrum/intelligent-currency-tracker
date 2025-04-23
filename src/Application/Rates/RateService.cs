using System.Globalization;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Utils;
using Application.Rates.Results;
using Domain.Common;
using Domain.Constants;
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
            await _rateHelper.GetRatesAsync(request.Start, request.End, request.Currency, request.Page,
                request.PageSize));

        return GetRatesResult.SuccessResult(ratesDto);
    }

    public async Task<BaseResult> DeleteRatesAsync(string date)
    {
        DateTime dateTime = DateTime.ParseExact(
            date, DateConstants.DateFormat, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        bool isDeleted = await _rateHelper.DeleteRatesAsync(dateTime);
        
        return isDeleted ? BaseResult.SuccessResult() : BaseResult.FailureResult(["Failed to delete rates."]);
    }
}