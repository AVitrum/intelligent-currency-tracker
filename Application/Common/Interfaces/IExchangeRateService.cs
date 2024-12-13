using Application.Common.Models;
using Domain.Common;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Interfaces;

public interface IExchangeRateService
{
    Task<BaseResult> FetchExchangeRatesAsync(ExchangeRatesRangeDto dto);
    Task<BaseResult> GetExchangeRatesFromCsvAsync(IFormFile? file);
    Task<BaseResult> ExportExchangeRatesToCsvAsync(ExchangeRatesRangeDto dto);
    Task<BaseResult> TrainModelAsync(ExchangeRatesRangeDto dto);
    Task<BaseResult> GetRangeAsync(ExchangeRatesRangeDto dto);
}