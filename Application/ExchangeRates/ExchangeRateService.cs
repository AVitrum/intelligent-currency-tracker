using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.ExchangeRates.Results;
using Domain.Common;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using Domain.Enums;
using IMapper = AutoMapper.IMapper;

namespace Application.ExchangeRates;

public class ExchangeRateService : IExchangeRateService
{
    private readonly HttpClient _httpClient;
    private readonly IExchangeRateRepository _exchangeRateRepository;
    private readonly ILogger<ExchangeRateService> _logger;
    private readonly IMapper _mapper;
    private readonly IAppSettings _appSettings;

    public ExchangeRateService(
        HttpClient httpClient,
        IExchangeRateRepository exchangeRateRepository,
        ILogger<ExchangeRateService> logger,
        IMapper mapper,
        IAppSettings appSettings)
    {
        _httpClient = httpClient;
        _exchangeRateRepository = exchangeRateRepository;
        _logger = logger;
        _mapper = mapper;
        _appSettings = appSettings;
    }

    public async Task<BaseResult> FetchExchangeRatesAsync(ExchangeRatesRangeDto dto)
    {
        if (!dto.TryGetDateRange(out var start, out var end))
            return BaseResult.FailureResult(["Invalid date format. Please use dd.MM.yyyy"]);
        
        _logger.LogInformation("Starting FetchExchangeRatesAsync from {StartDate} to {EndDate}", start, end);
        var collectedData = new List<ExchangeRate>();
        var currentDate = start;
        const int maxRetries = 3;

        while (currentDate <= end)
        {
            var formattedDate = currentDate.ToString("dd.MM.yyyy");
            var url = $"{_appSettings.PrivateBankUrl}/exchange_rates?json&date={formattedDate}";

            for (var attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    _logger.LogDebug("Fetching data for {Date}, attempt {Attempt}", formattedDate, attempt + 1);
                    var response = await _httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var jsonData = await response.Content.ReadAsStringAsync();
                    var exchangeRates = JObject.Parse(jsonData)["exchangeRate"]!;

                    collectedData.AddRange(exchangeRates.Select(rate => new ExchangeRate
                    {
                        Date = DateTime.Parse(((string)JObject.Parse(jsonData)["date"])!),
                        Currency = Enum.Parse<Currency>(((string)rate["currency"])!),
                        SaleRateNb = (decimal?)rate["saleRateNB"] ?? 0,
                        PurchaseRateNb = (decimal?)rate["purchaseRateNB"] ?? 0,
                        SaleRate = (decimal?)rate["saleRate"] ?? 0,
                        PurchaseRate = (decimal?)rate["purchaseRate"] ?? 0
                    }));

                    break;
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Error fetching data for {Date}, attempt {Attempt}", formattedDate, attempt + 1);
                    if (attempt == maxRetries - 1)
                    {
                        _logger.LogError("Max retries reached for {Date}", formattedDate);
                    }
                    else
                    {
                        await Task.Delay(1000);
                    }
                }
            }

            currentDate = currentDate.AddDays(1);
        }

        await _exchangeRateRepository.SaveExchangeRatesAsync(collectedData);
        _logger.LogInformation("Successfully fetched and saved exchange rates from {StartDate} to {EndDate}", start, end);
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

            if (!DateTime.TryParseExact(values[0], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
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

        var currency = dto.Currency ?? Currency.USD;
        
        try
        {
            var exchangeRates = await _exchangeRateRepository.GetExchangeRatesAsync(
                start.ToUniversalTime(), end.ToUniversalTime(), currency);
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

            var fileName = $"ExchangeRates_{currency.ToString()}_{start:yyyyMMdd}_{end:yyyyMMdd}.csv";
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

        _logger.LogInformation("Sending POST request to {Url}", $"{_appSettings.ModelUrl}/train");

        var response = await _httpClient.PostAsync($"{_appSettings.ModelUrl}/train", content);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Model training started successfully");
            return BaseResult.SuccessResult();
        }

        var errorMessage = await response.Content.ReadAsStringAsync();
        _logger.LogError("Failed to start model training: {ErrorMessage}", errorMessage);
        return BaseResult.FailureResult([errorMessage]);
    }

    public async Task<BaseResult> GetRangeAsync(ExchangeRatesRangeDto dto)
    {
        if (!dto.TryGetDateRange(out var start, out var end))
            return BaseResult.FailureResult(["Invalid date format. Please use dd.MM.yyyy"]);

        var currency = dto.Currency ?? Currency.USD;
        
        var exchangeRates = await _exchangeRateRepository.GetExchangeRatesAsync(
            start.ToUniversalTime(), end.ToUniversalTime(), currency);
        var exchangeRatesDto = exchangeRates.Select(exchangeRate =>
            _mapper.Map<ExchangeRateDto>(exchangeRate)).ToList();
        
        return GetExchangeRateRangeResult.SuccessResult(exchangeRatesDto);
    }
}