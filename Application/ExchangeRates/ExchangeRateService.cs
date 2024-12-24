using Application.Common.Interfaces;
using Application.Common.Models;
using Application.ExchangeRates.Results;
using AutoMapper;
using Confluent.Kafka;
using Domain.Common;
using Domain.Constans;
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
    
    public async Task<BaseResult> FetchExchangeRatesAsync(ExchangeRatesRangeDto dto)
    {
        if (!dto.TryGetDateRange(out DateTime start, out DateTime end))
            return BaseResult.FailureResult(["Invalid date format. Please use dd.MM.yyyy"]);

        _logger.LogInformation("Starting FetchExchangeRatesAsync from {StartDate} to {EndDate}", start, end);

        var dateRange = new List<string>();
        DateTime currentDate = start;
        while (currentDate <= end)
        {
            dateRange.Add(currentDate.ToString(DateConstants.DateFormat));
            currentDate = currentDate.AddDays(1);
        }

        var fetchRequest = new FetchRequest
        {
            Dates = dateRange,
            UrlTemplate = _appSettings.PrivateBankUrl + "/exchange_rates?json&date={0}"
        };

        var message = new Message<string, string>
        {
            Key = Guid.NewGuid().ToString(),
            Value = JsonConvert.SerializeObject(fetchRequest)
        };
        
        try
        {
            await _kafkaProducer.ProduceAsync("fetch-exchange-rates", message);
            _logger.LogInformation("Successfully sent fetch request to Kafka for dates {StartDate} to {EndDate}", start, end);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send fetch request to Kafka");
            return BaseResult.FailureResult(["Failed to send fetch request to Kafka"]);
        }

        return BaseResult.SuccessResult();
    }

    public async Task<BaseResult> GetRangeAsync(ExchangeRatesRangeDto dto)
    {
        if (!dto.TryGetDateRange(out DateTime start, out DateTime end))
            return BaseResult.FailureResult(["Invalid date format. Please use dd.MM.yyyy"]);
        
        if (dto.Currency is null) return BaseResult.FailureResult(["Currency type is required"]);

        var exchangeRates = await _exchangeRateRepository.GetExchangeRatesByCurrencyAsync(
            start.ToUniversalTime(), end.ToUniversalTime(), dto.Currency.Value);
        var exchangeRatesDto = exchangeRates.Select(exchangeRate =>
            _mapper.Map<ExchangeRateDto>(exchangeRate)).ToList();

        var exchangeRateListDto = new GetExchangeRateListDto(exchangeRatesDto);
        
        await _kafkaProducer.ProduceAsync("exchange-rates", new Message<string, string>
        {
            Key = "exchange-rates",
            Value = JsonConvert.SerializeObject(exchangeRateListDto)
        });
        
        return GetExchangeRateRangeResult.SuccessResult(exchangeRateListDto);
    }
}