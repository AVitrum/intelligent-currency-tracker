using Domain.Common;
using Shared.Dtos;

namespace Shared.Payload.Responses.Rate;

public class GetDetailsResponse : BaseResponse
{
    public GetDetailsResponse(
        bool success,
        string message,
        int statusCode,
        IEnumerable<string> errors,
        SingleCurrencyAnalyticsDto? details) : base(success, message, statusCode, errors)
    {
        Details = details;
    }

    public SingleCurrencyAnalyticsDto? Details { get; }
}