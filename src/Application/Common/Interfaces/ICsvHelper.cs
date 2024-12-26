using Application.Common.Models;
using Domain.Common;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Interfaces;

public interface ICsvHelper
{
    Task<BaseResult> ImportExchangeRateFromCsvAsync(IFormFile? file);
    Task<BaseResult> ExportExchangeRateToCsvAsync(ExchangeRatesRangeDto dto);
}