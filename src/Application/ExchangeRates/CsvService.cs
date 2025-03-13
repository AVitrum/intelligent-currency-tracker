using System.Globalization;
using System.Text;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Utils;
using Application.ExchangeRates.Results;
using CsvHelper;
using CsvHelper.Configuration;
using Domain.Common;
using Microsoft.Extensions.Logging;
using Shared.Dtos;
using Shared.Payload.Requests;

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
        ICollection<RateDto> ratesDto = (ICollection<RateDto>)_rateHelper.ConvertRatesToDtoAsync(
            await _rateHelper.GetRatesFromRequestAsync(request));

        if (ratesDto.Count == 0)
        {
            return BaseResult.FailureResult(["No rates found."]);
        }

        string fileName = $"ExchangeRates_{request.Currency}_{request.Start:yyyyMMdd}_{request.End:yyyyMMdd}.csv";
        byte[] fileContent = await CreateCsvFileAsync(ratesDto);

        return ExportExchangeRatesToCsvResult.SuccessResult(fileContent, fileName);
    }

    private async Task<byte[]> CreateCsvFileAsync<T>(IEnumerable<T> content)
    {
        try
        {
            using MemoryStream memoryStream = new();
            await using StreamWriter streamWriter = new(memoryStream, Encoding.UTF8);
            await using CsvWriter csvWriter = new(streamWriter, new CsvConfiguration(CultureInfo.InvariantCulture)
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
            const string message = "An error occurred while exporting rates to CSV file";
            _logger.LogError(ex, message);
            throw new ExportCsvException(message);
        }
    }
}