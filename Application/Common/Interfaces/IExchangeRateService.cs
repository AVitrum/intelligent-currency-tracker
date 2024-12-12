using Domain.Common;
using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Interfaces;

public interface IExchangeRateService
{
    Task<BaseResult> FetchExchangeRatesAsync(DateTime start, DateTime end);
    Task<BaseResult> GetExchangeRatesFromCsvAsync(IFormFile? file);
    Task<BaseResult> ExportExchangeRatesToCsvAsync(DateTime start, DateTime end, Currency currency);
}