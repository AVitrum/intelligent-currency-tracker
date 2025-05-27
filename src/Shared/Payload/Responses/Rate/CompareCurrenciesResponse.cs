using Domain.Common;
using Shared.Dtos;

namespace Shared.Payload.Responses.Rate;

public class CompareCurrenciesResponse : BaseResponse
{
    public CompareCurrenciesResponse(
        bool success,
        string message,
        int statusCode,
        IEnumerable<string> errors,
        ComparativeAnalyticsDto? comparativeAnalytics) : base(success, message, statusCode, errors)
    {
        ComparativeAnalytics = comparativeAnalytics;
    }

    public ComparativeAnalyticsDto? ComparativeAnalytics { get; }
}