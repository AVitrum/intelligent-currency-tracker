using Domain.Common;

namespace Shared.Payload.Responses.Identity;

public class SearchEmailsResponse : BaseResponse
{
    public IEnumerable<string> Data { get; set; }

    public SearchEmailsResponse(
        bool success,
        string message,
        int statusCode,
        IEnumerable<string> errors,
        IEnumerable<string> data) : base(success, message, statusCode, errors)
    {
        Data = data;
    }
}