using Application.Common.Interfaces;
using Application.Common.Models;
using Application.ExchangeRates.Results;
using AutoMapper;
using Confluent.Kafka;
using Domain.Common;
using Domain.Constans;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Application.ExchangeRates;

public class ExchangeRateService : IExchangeRateService
{
    private readonly IExchangeRateRepository _exchangeRateRepository;
    private readonly ILogger<ExchangeRateService> _logger;
    private readonly IMapper _mapper;
    private readonly IAppSettings _appSettings;
    private readonly IKafkaProducer _kafkaProducer;
    
    public ExchangeRateService(
        IExchangeRateRepository exchangeRateRepository,
        ILogger<ExchangeRateService> logger,
        IMapper mapper,
        IAppSettings appSettings,
        IKafkaProducer kafkaProducer)
    {
        _exchangeRateRepository = exchangeRateRepository;
        _logger = logger;
        _mapper = mapper;
        _appSettings = appSettings;
        _kafkaProducer = kafkaProducer;
    }
    
    public async Task<BaseResult> FetchExchangeRatesAsync(ExchangeRatesRangeDto exchangeRatesRangeDto)
    {
        _logger.LogInformation("Starting FetchExchangeRatesAsync from {StartDate} to {EndDate}", exchangeRatesRangeDto.Start, exchangeRatesRangeDto.End);

        var dateRangeList = new List<string>();
        DateTime currentDate = exchangeRatesRangeDto.Start;
        while (currentDate <= exchangeRatesRangeDto.End)
        {
            dateRangeList.Add(currentDate.ToString(DateConstants.DateFormat));
            currentDate = currentDate.AddDays(1);
        }

        var fetchRequest = new FetchRequest
        {
            Dates = dateRangeList,
            UrlTemplate = _appSettings.PrivateBankUrl + "/exchange_rates?json&date={0}"
        };

        var kafkaMessage = new Message<string, string>
        {
            Key = Guid.NewGuid().ToString(),
            Value = JsonConvert.SerializeObject(fetchRequest)
        };
        
        try
        {
            await _kafkaProducer.ProduceAsync("fetch-exchange-rates", kafkaMessage);
            _logger.LogInformation("Successfully sent fetch request to Kafka for dates {StartDate} to {EndDate}",
                exchangeRatesRangeDto.Start, exchangeRatesRangeDto.End);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send fetch request to Kafka");
            return BaseResult.FailureResult(["Failed to send fetch request to Kafka"]);
        }

        return BaseResult.SuccessResult();
    }

    public async Task<BaseResult> GetRangeAsync(ExchangeRatesRangeDto exchangeRatesRangeDto)
    {
        if (exchangeRatesRangeDto.Currency is null)
        {
            return BaseResult.FailureResult(["Currency type is required"]);
        }

        IEnumerable<ExchangeRate> exchangeRates =
            await _exchangeRateRepository.GetAllByStartDateAndEndDateAndCurrencyAsync(
                exchangeRatesRangeDto.Start.ToUniversalTime(), exchangeRatesRangeDto.End.ToUniversalTime(),
                exchangeRatesRangeDto.Currency);

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