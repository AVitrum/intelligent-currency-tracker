using Application.Common.Models;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Interfaces;

public interface ICsvHelper
{
    Task<bool> ImportExchangeRateFromCsvAsync(IFormFile? file);
    Task<(string, byte[])> ExportExchangeRateToCsvAsync(ExchangeRatesRangeDto dto);
}