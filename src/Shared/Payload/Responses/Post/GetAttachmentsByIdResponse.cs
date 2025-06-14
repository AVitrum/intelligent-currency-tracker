using Domain.Common;

namespace Shared.Payload.Responses.Post;

public class GetAttachmentsByIdResponse : BaseResponse
{
    public IEnumerable<string> Attachments { get; set; }

    public GetAttachmentsByIdResponse(
        bool success,
        string message,
        int statusCode,
        IEnumerable<string> errors,
        IEnumerable<string> attachments) : base(success, message, statusCode, errors)
    {
        Attachments = attachments;
    }
}