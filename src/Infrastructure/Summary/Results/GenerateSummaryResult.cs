using Domain.Common;
using Shared.Dtos;

namespace Infrastructure.Summary.Results;

public class GenerateSummaryResult : BaseResult
{
    private GenerateSummaryResult(bool success, IEnumerable<string> errors, SummaryDto summary) : base(success, errors)
    {
        Summary = summary;
    }

    public SummaryDto Summary { get; }

    public static GenerateSummaryResult SuccessResult(SummaryDto summary)
    {
        return new GenerateSummaryResult(true, [], summary);
    }
}