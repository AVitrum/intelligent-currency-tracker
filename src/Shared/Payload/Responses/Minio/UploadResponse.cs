using Domain.Common;

namespace Shared.Payload.Responses.Minio;

public class UploadResponse : BaseResponse
{
    public UploadResponse(bool success, string message, int statusCode, IEnumerable<string> errors) : base(success,
        message, statusCode, errors) { }
}