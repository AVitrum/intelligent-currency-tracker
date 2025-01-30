using Domain.Common;
using Shared.Payload.Requests;

namespace Application.Common.Interfaces;

public interface ICsvService
{
    Task<BaseResult> ExportExchangeRatesToCsvAsync(ExchangeRateRequest request);
}