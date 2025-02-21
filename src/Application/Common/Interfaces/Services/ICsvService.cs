using Domain.Common;
using Shared.Payload.Requests;

namespace Application.Common.Interfaces.Services;

public interface ICsvService
{
    Task<BaseResult> ExportExchangeRatesToCsvAsync(ExchangeRateRequest request);
}