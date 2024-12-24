using Application.Common.Models;
using Domain.Common;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Interfaces;

public interface ICsvExchangeRateService
{
    Task<BaseResult> GetExchangeRatesFromCsvAsync(IFormFile? file);
    Task<BaseResult> ExportExchangeRatesToCsvAsync(ExchangeRatesRangeDto dto);
}