using Application.Common.Interfaces;
using Application.Common.Models;
using Application.ExchangeRates.Results;
using Domain.Common;
using Microsoft.AspNetCore.Http;


namespace Application.ExchangeRates;

public class CsvExchangeRateService : ICsvExchangeRateService
{
    private readonly ICsvHelper _csvHelper;
    
    public CsvExchangeRateService(ICsvHelper csvHelper)
    {
        _csvHelper = csvHelper;
    }

    public async Task<BaseResult> GetExchangeRatesFromCsvAsync(IFormFile? file)
    {
        bool success = await _csvHelper.ImportExchangeRateFromCsvAsync(file);

        return success
            ? BaseResult.SuccessResult()
            : BaseResult.FailureResult(["Failed to import exchange rates from CSV"]);
    }

    public async Task<BaseResult> ExportExchangeRatesToCsvAsync(ExchangeRatesRangeDto dto)
    {
        (string name, byte[] content) = await _csvHelper.ExportExchangeRateToCsvAsync(dto);
        return ExportExchangeRatesToCsvResult.SuccessResult(content, name);
    }
}