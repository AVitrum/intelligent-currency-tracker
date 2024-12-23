using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.ExchangeRates.Results;
using AutoMapper;
using Confluent.Kafka;
using Domain.Common;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using CsvHelper;
using CsvHelper.Configuration;
using Domain.Enums;
using Newtonsoft.Json;

namespace Application.ExchangeRates;

public class ExchangeRateService : IExchangeRateService
{
    private readonly HttpClient _httpClient;
    private readonly IExchangeRateRepository _exchangeRateRepository;
    private readonly ILogger<ExchangeRateService> _logger;
    private readonly IMapper _mapper;
    private readonly IAppSettings _appSettings;
    private readonly IKafkaProducer _kafkaProducer;
    
    private const int MaxRetries = 3;
    private const string DateFormat = "dd.MM.yyyy";

    public ExchangeRateService(
        HttpClient httpClient,
        IExchangeRateRepository exchangeRateRepository,
        ILogger<ExchangeRateService> logger,
        IMapper mapper,
        IAppSettings appSettings,
        IKafkaProducer kafkaProducer)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromMinutes(5);
        _exchangeRateRepository = exchangeRateRepository;
        _logger = logger;
        _mapper = mapper;
        _appSettings = appSettings;
        _kafkaProducer = kafkaProducer;
    }
    
    public async Task<BaseResult> FetchExchangeRatesAsync(ExchangeRatesRangeDto dto)
    {
        if (!dto.TryGetDateRange(out var start, out var end))
            return BaseResult.FailureResult(["Invalid date format. Please use dd.MM.yyyy"]);

        _logger.LogInformation("Starting FetchExchangeRatesAsync from {StartDate} to {EndDate}", start, end);

        var dateRange = new List<string>();
        var currentDate = start;
        while (currentDate <= end)
        {
            dateRange.Add(currentDate.ToString(DateFormat));
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
    
    public async Task<BaseResult> GetExchangeRatesFromCsvAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            _logger.LogWarning("File is empty or null");
            return BaseResult.FailureResult(["File is empty"]);
        }

        _logger.LogInformation("Starting GetExchangeRatesFromCsvAsync");
        using var reader = new StreamReader(file.OpenReadStream());
        var csvData = new List<ExchangeRate>();

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("Date")) continue;

            var values = line.Split(',');

            if (values.Length != 6) continue;

            if (!DateTime.TryParseExact(values[0], DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                _logger.LogWarning("Invalid date format in CSV: {Line}", line);
                return BaseResult.FailureResult(["Invalid date format in CSV"]);
            }

            _logger.LogInformation($"Processing {date} exchange rates");

            var exchangeRate = new ExchangeRate
            {
                Date = date,
                Currency = Enum.Parse<Currency>(values[1]),
                SaleRateNb = decimal.TryParse(values[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var saleRateNb) ? saleRateNb : -1,
                PurchaseRateNb = decimal.TryParse(values[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var purchaseRateNb) ? purchaseRateNb : -1,
                SaleRate = decimal.TryParse(values[4], NumberStyles.Any, CultureInfo.InvariantCulture, out var saleRate) ? saleRate : -1,
                PurchaseRate = decimal.TryParse(values[5], NumberStyles.Any, CultureInfo.InvariantCulture, out var purchaseRate) ? purchaseRate : -1
            };

            csvData.Add(exchangeRate);
        }

        await _exchangeRateRepository.SaveExchangeRatesAsync(csvData);
        _logger.LogInformation("Successfully processed CSV file and saved exchange rates");
        return BaseResult.SuccessResult();
    }

    public async Task<BaseResult> ExportExchangeRatesToCsvAsync(ExchangeRatesRangeDto dto)
    {
        if (!dto.TryGetDateRange(out var start, out var end))
            return BaseResult.FailureResult(["Invalid date format. Please use dd.MM.yyyy"]);
        
        try
        {
            IEnumerable<ExchangeRate> exchangeRates;

            if (dto.Currency is null)
            {
                exchangeRates = await _exchangeRateRepository.GetExchangeRatesAsync(
                    start.ToUniversalTime(), end.ToUniversalTime());

                dto.Currency = Currency.ALL;
            }
            else
            {
                exchangeRates = await _exchangeRateRepository.GetExchangeRatesByCurrencyAsync(start.ToUniversalTime(),
                    end.ToUniversalTime(), dto.Currency.Value);
            }

            var exchangeRatesDto = exchangeRates.Select(exchangeRate =>
                _mapper.Map<ExchangeRateDto>(exchangeRate)).ToList();

            using var memoryStream = new MemoryStream();
            await using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
            await using var csvWriter = new CsvWriter(streamWriter, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ","
            });

            await csvWriter.WriteRecordsAsync(exchangeRatesDto);
            await streamWriter.FlushAsync();
            var fileContent = memoryStream.ToArray();

            var fileName = $"ExchangeRates_{dto.Currency.ToString()}_{start:yyyyMMdd}_{end:yyyyMMdd}.csv";
            _logger.LogInformation("Successfully exported exchange rates to CSV file");
            return ExportExchangeRatesToCsvResult.SuccessResult(fileContent, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while exporting exchange rates to CSV file");
            return BaseResult.FailureResult(["An error occurred while exporting exchange rates to CSV file"]);
        }
    }

    public async Task<BaseResult> TrainModelAsync(ExchangeRatesRangeDto dto)
    {
        _logger.LogInformation("Starting TrainModelAsync");

        var baseResult = await ExportExchangeRatesToCsvAsync(dto);

        if (baseResult is not ExportExchangeRatesToCsvResult exportResult)
        {
            _logger.LogWarning("Failed to export exchange rates to CSV");
            return BaseResult.FailureResult(["Failed to export exchange rates to CSV"]);
        }

        _logger.LogInformation("Successfully exported exchange rates to CSV");

        using var content = new MultipartFormDataContent();
        using var fileContentStream = new ByteArrayContent(exportResult.FileContent);
        fileContentStream.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        content.Add(fileContentStream, "file", exportResult.FileName);

        _logger.LogInformation("Sending POST request to {Url}", $"{_appSettings.ModelUrl}/train-model");

        var response = await _httpClient.PostAsync($"{_appSettings.ModelUrl}/train-model", content);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Model training started successfully");
            return BaseResult.SuccessResult();
        }

        var errorMessage = await response.Content.ReadAsStringAsync();
        _logger.LogError("Failed to start model training: {ErrorMessage}", errorMessage);
        return BaseResult.FailureResult([errorMessage]);
    }

    public async Task<BaseResult> PredictAsync(ExchangeRatePredictionDto dto)
    {
        var url = $"{_appSettings.ModelUrl}/predict/?pre_date={dto.PreDate}&currency_code={dto.CurrencyCode}";
        _logger.LogInformation("Sending GET request to {Url}", url);

        try
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Successfully received prediction");
            var prediction = await response.Content.ReadAsStringAsync();
            return ExchangeRatePredictionResult.SuccessResult(prediction);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to get prediction: {Message}", ex.Message);
            return BaseResult.FailureResult(["Failed to get prediction. Please try again later."]);
        }
    }

    public async Task<BaseResult> GetRangeAsync(ExchangeRatesRangeDto dto)
    {
        if (!dto.TryGetDateRange(out var start, out var end))
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