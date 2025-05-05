using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Utils;
using Application.Rates.Results;
using AutoMapper;
using Domain.Common;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Shared.Dtos;
using Shared.Helpers;

namespace Application.Rates;

public class RateService : IRateService
{
    private readonly IRateRepository _rateRepository;
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IRateHelper _rateHelper;
    private readonly ILogger<RateService> _logger;
    private readonly IMapper _mapper;
    
    public RateService(
        IRateRepository rateRepository,
        ICurrencyRepository currencyRepository,
        IRateHelper rateHelper,
        ILogger<RateService> logger, 
        IMapper mapper)
    {
        _rateRepository = rateRepository;
        _currencyRepository = currencyRepository;
        _rateHelper = rateHelper;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<BaseResult> GetRatesAsync(
        DateTime start,
        DateTime end,
        string? currencyString,
        int page,
        int pageSize)
    {
        IEnumerable<Rate> rates = await GetRatesFromDbAsync(start, end, currencyString, page, pageSize);
        
        IEnumerable<RateDto> ratesDto = _rateHelper.ConvertRatesToDtoAsync(rates);

        return GetRatesResult.SuccessResult(ratesDto);
    }

    public async Task<BaseResult> GetAllCurrenciesAsync()
    {
        ICollection<Currency> currencies = (ICollection<Currency>) await _currencyRepository.GetAllAsync();
        
        if (currencies is null || currencies.Count == 0)
        {
            throw new EntityNotFoundException<Currency>();
        }

        List<CurrencyDto> currenciesDto = [];
        currenciesDto.AddRange(currencies.Select(currency => _mapper.Map<CurrencyDto>(currency)));
        currenciesDto = currenciesDto.OrderBy(c => c.Code).ToList();

        _logger.LogInformation("Successfully retrieved all currencies");
        
        return GetAllCurrenciesResult.SuccessResult(currenciesDto);
    }

    public async Task<BaseResult> GetLastUpdatedCurrenciesAsync()
    {
        List<Rate> rates = (List<Rate>)await _rateRepository.GetLastUpdatedAsync();
        
        if (rates.Count == 0)
        {
            return BaseResult.FailureResult(["No rates found."]);
        }

        ICollection<Currency> currencies = [];
        
        foreach (Rate rate in rates)
        {
            Currency? currency = await _currencyRepository.GetByIdAsync(rate.CurrencyId);
            if (currency is null)
            {
                throw new EntityNotFoundException<Currency>();
            }

            currencies.Add(currency);
        }
        
        IEnumerable<CurrencyDto> currencyDtos = _rateHelper.ConvertCurrenciesToDtoAsync(currencies);
        return GetAllCurrenciesResult.SuccessResult(currencyDtos);
    }

    public async Task<BaseResult> DeleteRatesAsync(string date)
    {
        DateTime dateTime = DateHelper.ParseDdMmYyyy(date);
        bool isDeleted = await _rateRepository.RemoveByDateAsync(dateTime);
        
        return isDeleted ? BaseResult.SuccessResult() : BaseResult.FailureResult(["Failed to delete rates."]);
    }

    private async Task<IEnumerable<Rate>> GetRatesFromDbAsync(
        DateTime start,
        DateTime end,
        string? currencyString,
        int page,
        int pageSize)
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

            if (page == 0 || pageSize == 0)
            {
                rates = await _rateRepository.GetRangeAsync(start, end, currency);
            }
            else 
            {
                rates = await _rateRepository.GetRangeAsync(start, end, currency, page, pageSize);
            }
        }

        return rates;
    }
}