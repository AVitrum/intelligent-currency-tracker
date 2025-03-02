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

    public async Task<IEnumerable<Rate>> GetRatesFromRequestAsync(ExchangeRateRequest request)
    {
        IEnumerable<Rate> rates;
        if (request.Currency is null)
        {
            rates = await _rateRepository.GetAsync(request.Start);
        }
        else
        {
            var currency = await _currencyRepository.GetByCodeAsync(request.Currency)
                           ?? throw new EntityNotFoundException<Currency>();

            rates = await _rateRepository.GetAsync(request.Start, request.End, currency, request.Page,
                request.PageSize);
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