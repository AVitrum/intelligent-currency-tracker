using Domain.Common;
using Shared.Dtos;

namespace Shared.Payload.Responses;

public class GetAllReportsResponse : BaseResponse
{
    public IEnumerable<ReportDto> Reports { get; set; }

    public GetAllReportsResponse(
        bool success,
        string message,
        int statusCode,
        IEnumerable<string> errors,
        IEnumerable<ReportDto> reports) : base(success, message, statusCode, errors)
    {
        Reports = reports;
    }
}