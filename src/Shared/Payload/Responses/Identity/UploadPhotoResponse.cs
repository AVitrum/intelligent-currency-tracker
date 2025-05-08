using Domain.Common;

namespace Shared.Payload.Responses.Identity;

public class UploadPhotoResponse : BaseResponse
{
    public UploadPhotoResponse(bool success, string message, int statusCode, IEnumerable<string> errors) : base(success,
        message, statusCode, errors) { }
}