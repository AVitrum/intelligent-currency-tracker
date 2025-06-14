using Domain.Common;
using Shared.Dtos;

namespace Shared.Payload.Responses.Report;

public class GetReportByIdResponse : BaseResponse
{
    public ReportDto Report { get; set; }

    public GetReportByIdResponse(
        bool success,
        string message,
        int statusCode,
        IEnumerable<string> errors,
        ReportDto report) : base(success, message, statusCode, errors)
    {
        Report = report;
    }
}