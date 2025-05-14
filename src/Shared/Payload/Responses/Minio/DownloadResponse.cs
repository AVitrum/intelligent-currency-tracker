using Domain.Common;

namespace Shared.Payload.Responses.Minio;

public class DownloadResponse : BaseResponse
{
    public DownloadResponse(bool success, string message, int statusCode, IEnumerable<string> errors) : base(success,
        message, statusCode, errors) { }
}