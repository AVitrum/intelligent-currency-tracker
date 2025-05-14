using Domain.Common;

namespace Application.Common.Interfaces.Services;

public interface ISummaryService
{
    Task<BaseResult> GenerateSummaryAsync(DateTime startDate, DateTime endDate, string currencyCode);
}