using Domain.Common;

namespace Application.Posts.Results;

public class GetAttachmentsByIdResult : BaseResult
{
    public IEnumerable<string> Attachments { get; }

    private GetAttachmentsByIdResult(bool success, IEnumerable<string> errors, IEnumerable<string> attachments) : base(
        success, errors)
    {
        Attachments = attachments;
    }

    public static GetAttachmentsByIdResult SuccessResult(IEnumerable<string> attachments)
    {
        return new GetAttachmentsByIdResult(true, [], attachments);
    }
}