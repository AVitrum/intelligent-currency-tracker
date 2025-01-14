using Application.Common.Interfaces;
using Application.Common.Payload.Dtos;
using Application.Common.Payload.Requests;
using Application.Rates.Results;
using AutoMapper;
using Domain.Common;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Rates;

public class RateService : IRateService
{
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IMapper _mapper;
    private readonly IRateRepository _rateRepository;

    public RateService(IMapper mapper, ICurrencyRepository currencyRepository, IRateRepository rateRepository)
    {
        _mapper = mapper;
        _currencyRepository = currencyRepository;
        _rateRepository = rateRepository;
    }

    public async Task<BaseResult> GetRatesAsync(ExchangeRateRequest request)
    {
        IEnumerable<Rate> rates;
        var ratesDto = new List<RateDto>();

        if (request.Currency is null)
        {
            rates = await _rateRepository.GetAsync(request.Start, request.End);
        }
        else
        {
            var currency = await _currencyRepository.GetByCodeAsync(request.Currency)
                           ?? throw new EntityNotFoundException<Currency>();
            rates = await _rateRepository.GetAsync(request.Start, request.End, currency);
        }

        ratesDto.AddRange(rates.Select(rate => _mapper.Map<RateDto>(rate)));
        return GetRatesResult.SuccessResult(ratesDto);
    }
}