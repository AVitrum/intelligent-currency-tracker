using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Utils;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Shared.Dtos;
using Shared.Payload.Requests;

namespace Application.Common.Helpers;

public class RateHelper : IRateHelper
{
    private readonly ICurrencyRepository _currencyRepository;
    private readonly ILogger<RateHelper> _logger;
    private readonly IMapper _mapper;
    private readonly IRateRepository _rateRepository;

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

    public async Task<IEnumerable<Rate>> GetRatesAsync(DateTime start, DateTime end, string? currencyString, int page, int pageSize)
    {
        IEnumerable<Rate> rates;
        if (currencyString is null)
        {
            if (start.Date == end.Date)
            {
                rates = await _rateRepository.GetRangeAsync(start);
            }
            else
            {
                rates = await _rateRepository.GetRangeAsync(start, end, page, pageSize);
            }
        }
        else
        {
            Currency currency = await _currencyRepository.GetByCodeAsync(currencyString)
                                ?? throw new EntityNotFoundException<Currency>();

            rates = await _rateRepository.GetRangeAsync(start, end, currency, page, pageSize);
        }

        return rates;
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
}