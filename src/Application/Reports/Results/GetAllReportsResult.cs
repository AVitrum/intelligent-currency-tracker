using Domain.Common;
using Shared.Dtos;

namespace Application.Reports.Results;

public class GetAllReportsResult : BaseResult
{
    public IEnumerable<ReportDto> Reports { get; set; }

    private GetAllReportsResult(bool success, IEnumerable<string> errors, IEnumerable<ReportDto> reports) : base(
        success, errors)
    {
        Reports = reports;
    }

    public static GetAllReportsResult SuccessResult(IEnumerable<ReportDto> reports)
    {
        return new GetAllReportsResult(true, [], reports);
    }
}