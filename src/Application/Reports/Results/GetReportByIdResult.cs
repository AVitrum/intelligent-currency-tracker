using Domain.Common;
using Shared.Dtos;

namespace Application.Reports.Results;

public class GetReportByIdResult : BaseResult
{
    public ReportDto ReportDto { get; set; }

    private GetReportByIdResult(bool success, IEnumerable<string> errors, ReportDto reportDto) : base(success, errors)
    {
        ReportDto = reportDto;
    }

    public static GetReportByIdResult SuccessResult(ReportDto reportDto)
    {
        return new GetReportByIdResult(true, [], reportDto);
    }
}