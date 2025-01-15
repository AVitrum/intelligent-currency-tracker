using System.Globalization;
using System.Text;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Payload.Dtos;
using Application.Common.Payload.Requests;
using Application.ExchangeRates.Results;
using CsvHelper;
using CsvHelper.Configuration;
using Domain.Common;
using Microsoft.Extensions.Logging;

namespace Application.ExchangeRates;

public class CsvService : ICsvService
{
    private readonly ILogger<CsvService> _logger;
    private readonly IRateHelper _rateHelper;

    public CsvService(ILogger<CsvService> logger, IRateHelper rateHelper)
    {
        _logger = logger;
        _rateHelper = rateHelper;
    }

    public async Task<BaseResult> ExportExchangeRatesToCsvAsync(ExchangeRateRequest request)
    {
        var ratesDto = (ICollection<RateDto>)_rateHelper.ConvertRatesToDtoAsync(
            await _rateHelper.GetRatesFromRequestAsync(request));
        if (ratesDto.Count == 0) return BaseResult.FailureResult(["No rates found."]);

        var fileName = $"ExchangeRates_{request.Currency}_{request.Start:yyyyMMdd}_{request.End:yyyyMMdd}.csv";
        var fileContent = await CreateCsvFileAsync(ratesDto);

        return ExportExchangeRatesToCsvResult.SuccessResult(fileContent, fileName);
    }

    private async Task<byte[]> CreateCsvFileAsync<T>(IEnumerable<T> content)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while exporting rates to CSV file");
            throw new ExportCsvException("An error occurred while exporting rates to CSV file");
        }
    }
}