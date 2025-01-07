using System.Globalization;
using System.Text;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Payload.Dtos;
using Application.Common.Payload.Requests;
using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using Domain.Constans;
using Domain.Entities;
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

    public async Task<bool> ImportExchangeRateFromCsvAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            _logger.LogWarning("File is empty or null");
            throw new ArgumentNullException(nameof(file));
        }

        _logger.LogInformation("Starting GetExchangeRatesFromCsvAsync");

        List<ExchangeRate> csvData = await ReadCsvFileAsync(file) 
                                     ?? throw new ImportCsvException("Invalid date format in CSV");

        await _exchangeRateRepository.AddExchangeRateRangeAsync(csvData);
        _logger.LogInformation("Successfully processed CSV file and saved exchange rates");
        return true;
    }
        
    public async Task<(string, byte[])> ExportExchangeRateToCsvAsync(ExchangeRateRequest request)
    {
        try
        {
            IEnumerable<ExchangeRate> exchangeRates;

            DateTime startUtc = request.Start.ToUniversalTime();
            DateTime endUtc = request.End.ToUniversalTime();
            
            if (request.Currency is null)
            {
                exchangeRates = await _exchangeRateRepository.GetAllByStartDateAndEndDateAsync(startUtc, endUtc);
            }
            else
            {
                exchangeRates =
                    await _exchangeRateRepository.GetAllByStartDateAndEndDateAndCurrencyAsync(startUtc, endUtc,
                        request.Currency);
            }
            var exchangeRatesDto = exchangeRates.Select(exchangeRate =>
                _mapper.Map<ExchangeRateDto>(exchangeRate)).ToList();
            
            if (exchangeRatesDto.Count == 0)
            {
                throw new DataNotFoundException("No exchange rates found for the specified date range");
            }
                
            var fileName = $"ExchangeRates_{request.Currency}_{request.Start:yyyyMMdd}_{request.End:yyyyMMdd}.csv";
            byte[] fileContent = await CreateCsvFileAsync(exchangeRatesDto);
            _logger.LogInformation("Successfully exported exchange rates to CSV file");
            return (fileName, fileContent);
        }
        catch (DataNotFoundException ex)
        {
            _logger.LogWarning(ex, "No exchange rates found for the specified date range");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while exporting exchange rates to CSV file");
            throw new ExportCsvException("An error occurred while exporting exchange rates to CSV file");
        }
    }
        
    private async Task<List<ExchangeRate>?> ReadCsvFileAsync(IFormFile file)
    {
        var csvData = new List<ExchangeRate>();
        using var reader = new StreamReader(file.OpenReadStream());

        while (!reader.EndOfStream)
        {
            string? line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("Date")) continue;

            ExchangeRate? exchangeRate = ParseCsvLine(line);
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
        string[] values = line.Split(',');

        if (values.Length != 6) return null;

        if (!DateTime.TryParseExact(values[0], DateConstants.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
        {
            return null;
        }

        _logger.LogInformation($"Processing {date} exchange rates");

        return new ExchangeRate
        {
            Date = date,
            Currency = values[1],
            SaleRateNb = decimal.TryParse(values[2], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal saleRateNb) ? saleRateNb : -1,
            PurchaseRateNb = decimal.TryParse(values[3], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal purchaseRateNb) ? purchaseRateNb : -1,
            SaleRate = decimal.TryParse(values[4], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal saleRate) ? saleRate : -1,
            PurchaseRate = decimal.TryParse(values[5], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal purchaseRate) ? purchaseRate : -1
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