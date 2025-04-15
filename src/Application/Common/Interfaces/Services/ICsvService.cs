using Domain.Common;

namespace Application.Common.Interfaces.Services;

public interface ICsvService
{
    Task<BaseResult> ExportExchangeRatesToCsvAsync(DateTime start, DateTime end, int currencyR030);
}