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
    private readonly ICurrencyRepository _currencyRepository;
    private readonly ILogger<RateService> _logger;
    private readonly IMapper _mapper;
    private readonly IRateHelper _rateHelper;
    private readonly IRateRepository _rateRepository;

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
        ICollection<Currency> currencies = (ICollection<Currency>)await _currencyRepository.GetAllAsync();

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

    public async Task<BaseResult> GetAllCurrenciesAsync(DateTime start, DateTime end)
    {
        IEnumerable<Rate> rates = await GetRatesFromDbAsync(start, end, null, 0, 0);
        List<Currency> currencies = [];

        foreach (Rate rate in rates)
        {
            Currency? currency = await _currencyRepository.GetByIdAsync(rate.CurrencyId);
            if (currency is null)
            {
                throw new EntityNotFoundException<Currency>();
            }

            if (currencies.All(c => c.Id != currency.Id))
            {
                currencies.Add(currency);
            }
        }
        
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

    public async Task<BaseResult> GetCrossRatesAsync(string? currency1, string? currency2, DateTime requestStart, DateTime requestEnd)
    {
        if (string.IsNullOrEmpty(currency1) || string.IsNullOrEmpty(currency2))
        {
            return BaseResult.FailureResult(["Both currency codes are required for cross rates."]);
        }
        
        if (currency1 == currency2)
        {
            return BaseResult.FailureResult(["Both currency codes must be different for cross rates."]);
        }
        
        Currency c1 = await _currencyRepository.GetByCodeAsync(currency1) ??
                            throw new EntityNotFoundException<Currency>();
        Currency c2 = await _currencyRepository.GetByCodeAsync(currency2) ??
                            throw new EntityNotFoundException<Currency>();
        
        List<Rate> rates1 = (List<Rate>)await _rateRepository.GetRangeAsync(requestStart, requestEnd, c1);
        List<Rate> rates2 = (List<Rate>)await _rateRepository.GetRangeAsync(requestStart, requestEnd, c2);
     
        if (rates1.Count == 0 || rates2.Count == 0)
        {
            return BaseResult.FailureResult(["No rates found for the specified currencies and date range."]);
        }

        List<CrossRateDto> crossRates = new List<CrossRateDto>();
        Dictionary<DateTime, Rate> rates2Lookup = rates2.ToDictionary(r => r.Date.Date);

        foreach (Rate rate1 in rates1)
        {
            if (rates2Lookup.TryGetValue(rate1.Date.Date, out Rate? rate2))
            {
                if (rate2.Value == 0)
                {
                    _logger.LogWarning("Official rate for currency {CurrencyCode} on {Date} is zero. Skipping cross rate calculation for this date.", c2.Code, rate2.Date.ToShortDateString());
                    continue;
                }
                
                var crossRateValue = rate1.Value / rate2.Value;
                crossRates.Add(new CrossRateDto
                {
                    Date = rate1.Date,
                    SourceCurrencyCode = c1.Code,
                    TargetCurrencyCode = c2.Code,
                    Rate = crossRateValue
                });
            }
        }

        if (crossRates.Count == 0)
        {
            _logger.LogInformation(
                "No matching dates found for rates to calculate cross rates between {Currency1} and {Currency2} for the period {StartDate} - {EndDate}.",
                c1.Code, c2.Code, requestStart, requestEnd);
            return BaseResult.FailureResult(["No cross rates found for the specified currencies and date range."]);
        }
        
        return GetCrossRatesResult.SuccessResult(crossRates.OrderBy(cr => cr.Date));
    }

    public async Task<BaseResult> GetDetailsAsync(string currencyCode, DateTime start, DateTime end)
    {
        Currency currency = await _currencyRepository.GetByCodeAsync(currencyCode) ??
                            throw new EntityNotFoundException<Currency>();
        List<Rate> rates = (List<Rate>)await _rateRepository.GetRangeAsync(start, end, currency);

        if (rates.Count == 0)
        {
            return BaseResult.FailureResult(["No rates found for the specified currency and date range."]);
        }

        SingleCurrencyAnalyticsDto analysisDto = _rateHelper.AnalyzeCurrency(rates, currency.Code);
        return GetDetailsResult.SuccessResult(analysisDto);
    }

    public async Task<BaseResult> CompareCurrenciesAsync(List<string> currencyCodes, DateTime start, DateTime end)
    {
        if (currencyCodes.Count < 2)
        {
            return BaseResult.FailureResult(["At least two currency codes are required for comparison."]);
        }

        List<Currency> currencies = [];
        List<List<Rate>> ratesLists = [];
        List<SingleCurrencyAnalyticsDto> analysis = [];

        foreach (string code in currencyCodes)
        {
            Currency? currency = await _currencyRepository.GetByCodeAsync(code);
            if (currency is null)
            {
                return BaseResult.FailureResult([$"Currency with code {code} not found."]);
            }

            currencies.Add(currency);
            List<Rate> rates = (List<Rate>)await _rateRepository.GetRangeAsync(start, end, currency);
            if (rates.Count == 0)
            {
                return BaseResult.FailureResult([$"No rates found for currency {code} in the specified date range."]);
            }

            ratesLists.Add(rates);
        }

        analysis.AddRange(from currency in currencies
            let index = currencies.IndexOf(currency)
            let rates = ratesLists[index]
            select _rateHelper.AnalyzeCurrency(rates, currency.Code));

        ComparativeAnalyticsDto comparativeAnalytics = _rateHelper.CompareCurrencies(analysis, ratesLists, start, end);
        return CompareCurrenciesResult.SuccessResult(comparativeAnalytics);
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
            else if (page == 0 || pageSize == 0)
            {
                rates = await _rateRepository.GetRangeAsync(start, end);
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