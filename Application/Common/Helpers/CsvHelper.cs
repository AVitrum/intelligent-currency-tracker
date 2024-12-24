using System.Globalization;
using System.Text;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.ExchangeRates.Results;
using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using Domain.Common;
using Domain.Constans;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Application.Common.Helpers;

public class CsvHelper : ICsvHelper
{
    private readonly IExchangeRateRepository _exchangeRateRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CsvHelper> _logger;
        
    public CsvHelper(IExchangeRateRepository exchangeRateRepository, IMapper mapper, ILogger<CsvHelper> logger)
    {
        _exchangeRateRepository = exchangeRateRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseResult> ImportExchangeRateFromCsvAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            _logger.LogWarning("File is empty or null");
            return BaseResult.FailureResult(["File is empty"]);
        }

        _logger.LogInformation("Starting GetExchangeRatesFromCsvAsync");

        var csvData = await ReadCsvFileAsync(file);
        if (csvData == null)
        {
            return BaseResult.FailureResult(new[] { "Invalid date format in CSV" });
        }

        await _exchangeRateRepository.SaveExchangeRatesAsync(csvData);
        _logger.LogInformation("Successfully processed CSV file and saved exchange rates");
        return BaseResult.SuccessResult();
    }
        
    public async Task<BaseResult> ExportExchangeRateToCsvAsync(ExchangeRatesRangeDto dto)
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
                exchangeRates = await _exchangeRateRepository.GetExchangeRatesByCurrencyAsync(
                    start.ToUniversalTime(),
                    end.ToUniversalTime(), dto.Currency.Value);
            }
            var exchangeRatesDto = exchangeRates.Select(exchangeRate =>
                _mapper.Map<ExchangeRateDto>(exchangeRate)).ToList();
                
            var fileContent = await CreateCsvFileAsync(exchangeRatesDto);
            var fileName = $"ExchangeRates_{dto.Currency.ToString()}_{start:yyyyMMdd}_{end:yyyyMMdd}.csv";
                
            return ExportExchangeRatesToCsvResult.SuccessResult(fileContent, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while exporting exchange rates to CSV file");
            return BaseResult.FailureResult(["An error occurred while exporting exchange rates to CSV file"]);
        }
    }
        
    private async Task<List<ExchangeRate>?> ReadCsvFileAsync(IFormFile file)
    {
        var csvData = new List<ExchangeRate>();
        using var reader = new StreamReader(file.OpenReadStream());

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("Date")) continue;

            var exchangeRate = ParseCsvLine(line);
            if (exchangeRate == null)
            {
                _logger.LogWarning("Invalid date format in CSV: {Line}", line);
                return null;
            }

            csvData.Add(exchangeRate);
        }

        return csvData;
    }

    private ExchangeRate? ParseCsvLine(string line)
    {
        var values = line.Split(',');

        if (values.Length != 6) return null;

        if (!DateTime.TryParseExact(values[0], DateConstants.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return null;
        }

        _logger.LogInformation($"Processing {date} exchange rates");

        return new ExchangeRate
        {
            Date = date,
            Currency = Enum.Parse<Currency>(values[1]),
            SaleRateNb = decimal.TryParse(values[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var saleRateNb) ? saleRateNb : -1,
            PurchaseRateNb = decimal.TryParse(values[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var purchaseRateNb) ? purchaseRateNb : -1,
            SaleRate = decimal.TryParse(values[4], NumberStyles.Any, CultureInfo.InvariantCulture, out var saleRate) ? saleRate : -1,
            PurchaseRate = decimal.TryParse(values[5], NumberStyles.Any, CultureInfo.InvariantCulture, out var purchaseRate) ? purchaseRate : -1
        };
    }

    private async Task<byte[]> CreateCsvFileAsync<T>(List<T> content)
    {
        using var memoryStream = new MemoryStream();
        await using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
        await using var csvWriter = new CsvWriter(streamWriter, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ","
        });

        await csvWriter.WriteRecordsAsync(content);
        await streamWriter.FlushAsync();
            
        _logger.LogInformation("Successfully created CSV file");
        return memoryStream.ToArray();
    }
}