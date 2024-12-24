using Application.Common.Interfaces;
using Application.Common.Models;
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
        return await _csvHelper.ImportExchangeRateFromCsvAsync(file);
    }

    public async Task<BaseResult> ExportExchangeRatesToCsvAsync(ExchangeRatesRangeDto dto)
    {
        return await _csvHelper.ExportExchangeRateToCsvAsync(dto);
    }
}