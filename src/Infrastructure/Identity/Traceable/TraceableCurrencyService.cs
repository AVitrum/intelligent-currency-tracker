using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using AutoMapper;
using Domain.Common;
using Infrastructure.Identity.SubUserEntities;
using Infrastructure.Identity.Traceable.Results;
using Infrastructure.Interfaces;
using Shared.Dtos;

namespace Infrastructure.Identity.Traceable;

public class TraceableCurrencyService : ITraceableCurrencyService
{
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IMapper _mapper;
    private readonly ITraceableCurrencyRepository _traceableCurrencyRepository;

    public TraceableCurrencyService(
        ITraceableCurrencyRepository traceableCurrencyRepository,
        ICurrencyRepository currencyRepository,
        IMapper mapper)
    {
        _traceableCurrencyRepository = traceableCurrencyRepository;
        _currencyRepository = currencyRepository;
        _mapper = mapper;
    }

    public async Task<BaseResult> TrackCurrencyAsync(string userId, int currencyR030)
    {
        Currency? currency = await _currencyRepository.GetByR030Async(currencyR030);

        if (currency is null)
        {
            return BaseResult.FailureResult(["Currency not found"]);
        }

        TraceableCurrency newTraceableCurrency = new TraceableCurrency
        {
            CurrencyId = currency.Id,
            UserId = userId
        };

        await _traceableCurrencyRepository.AddAsync(newTraceableCurrency);

        return BaseResult.SuccessResult();
    }

    public async Task<BaseResult> GetTrackedCurrenciesAsync(string userId)
    {
        List<TraceableCurrency> trackedCurrencies =
            (List<TraceableCurrency>)await _traceableCurrencyRepository.GetByUserIdAsync(userId);

        if (trackedCurrencies.Count == 0)
        {
            return BaseResult.FailureResult(["No tracked currencies found"]);
        }

        IEnumerable<CurrencyDto> currencies = trackedCurrencies
            .Select(tc => tc.Currency)
            .Select(c => _mapper.Map<CurrencyDto>(c))
            .ToList();

        return GetAllResult.SuccessResult(currencies);
    }
}