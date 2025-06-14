using Domain.Common;
using Shared.Dtos;

namespace Application.Rates.Results;

public class GetDetailsResult : BaseResult
{
    private GetDetailsResult(bool success, IEnumerable<string> errors, SingleCurrencyAnalyticsDto details) : base(
        success, errors)
    {
        Details = details;
    }

    public SingleCurrencyAnalyticsDto Details { get; }

    public static GetDetailsResult SuccessResult(SingleCurrencyAnalyticsDto details)
    {
        return new GetDetailsResult(true, [], details);
    }
}