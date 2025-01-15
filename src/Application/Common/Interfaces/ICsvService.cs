using Application.Common.Payload.Requests;
using Domain.Common;

namespace Application.Common.Interfaces;

public interface ICsvService
{
    Task<BaseResult> ExportExchangeRatesToCsvAsync(ExchangeRateRequest request);
}