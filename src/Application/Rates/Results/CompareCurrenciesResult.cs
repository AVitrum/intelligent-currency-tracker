using Domain.Common;
using Shared.Dtos;

namespace Application.Rates.Results;

public class CompareCurrenciesResult : BaseResult
{
    private CompareCurrenciesResult(
        bool success,
        IEnumerable<string> errors,
        ComparativeAnalyticsDto comparativeAnalytics) : base(success, errors)
    {
        ComparativeAnalytics = comparativeAnalytics;
    }

    public ComparativeAnalyticsDto ComparativeAnalytics { get; }

    public static CompareCurrenciesResult SuccessResult(ComparativeAnalyticsDto comparativeAnalytics)
    {
        return new CompareCurrenciesResult(true, [], comparativeAnalytics);
    }
}