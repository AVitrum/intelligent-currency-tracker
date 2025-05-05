using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Utils;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Shared.Dtos;

namespace Application.Common.Helpers;

public class RateHelper : IRateHelper
{
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IRateRepository _rateRepository;
    private readonly ILogger<RateHelper> _logger;
    private readonly IMapper _mapper;

    public RateHelper(
        ICurrencyRepository currencyRepository,
        IRateRepository rateRepository,
        IMapper mapper,
        ILogger<RateHelper> logger)
    {
        _currencyRepository = currencyRepository;
        _rateRepository = rateRepository;
        _mapper = mapper;
        _logger = logger;
    }
    
    public async Task<IEnumerable<Rate>> GetRatesAsync(DateTime start, DateTime end, int currencyR030)
    {
        IEnumerable<Rate> rates;
        if (currencyR030 == 0)
        {
            if (start.Date == end.Date)
            {
                rates = await _rateRepository.GetRangeAsync(start);
            }
            else
            {
                rates = await _rateRepository.GetRangeAsync(start, end);
            }
        }
        else
        {
            Currency currency = await _currencyRepository.GetByR030Async(currencyR030)
                                ?? throw new EntityNotFoundException<Currency>();

            rates = await _rateRepository.GetRangeAsync(start, end, currency);
        }

        return rates;
    }

    public IEnumerable<RateDto> ConvertRatesToDtoAsync(IEnumerable<Rate> rates)
    {
        List<RateDto> ratesDto = [];
        ratesDto.AddRange(rates.Select(rate => _mapper.Map<RateDto>(rate)));

        _logger.LogInformation("Successfully converted rates to DTO");
        return ratesDto;
    }

    public IEnumerable<CurrencyDto> ConvertCurrenciesToDtoAsync(IEnumerable<Currency> rates)
    {
        List<CurrencyDto> currenciesDto = [];
        currenciesDto.AddRange(rates.Select(currency => _mapper.Map<CurrencyDto>(currency)));

        _logger.LogInformation("Successfully converted currencies to DTO");
        return currenciesDto;
    }
}