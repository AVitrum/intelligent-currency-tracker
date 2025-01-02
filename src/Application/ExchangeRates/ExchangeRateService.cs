using Application.Common.Interfaces;
using Application.Common.Payload.Dtos;
using Application.Common.Payload.Requests;
using Application.ExchangeRates.Results;
using AutoMapper;
using Domain.Common;
using Domain.Entities;

namespace Application.ExchangeRates;

public class ExchangeRateService : IExchangeRateService
{
    private readonly IExchangeRateRepository _exchangeRateRepository;
    private readonly IMapper _mapper;

    public ExchangeRateService(
        IExchangeRateRepository exchangeRateRepository,
        IMapper mapper)
    {
        _exchangeRateRepository = exchangeRateRepository;
        _mapper = mapper;
    }
    
    public async Task<BaseResult> GetRangeAsync(ExchangeRateRequest request)
    {
        if (request.Currency is null)
        {
            return BaseResult.FailureResult(["Currency type is required"]);
        }

        IEnumerable<ExchangeRate> exchangeRates =
            await _exchangeRateRepository.GetAllByStartDateAndEndDateAndCurrencyAsync(
                request.Start.ToUniversalTime(), request.End.ToUniversalTime(),
                request.Currency);

        var exchangeRateList = exchangeRates.ToList();
        if (exchangeRateList.Count == 0)
        {
            return GetExchangeRateRangeResult.FailureNotFoundResult();
        }
        
        var exchangeRateDtoList = exchangeRateList.Select(exchangeRate =>
            _mapper.Map<ExchangeRateDto>(exchangeRate)).ToList();
        var exchangeRateListDto = new GetExchangeRateListDto(exchangeRateDtoList);

        return GetExchangeRateRangeResult.SuccessResult(exchangeRateListDto);
    }
}