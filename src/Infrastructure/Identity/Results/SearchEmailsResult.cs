using Domain.Common;

namespace Infrastructure.Identity.Results;

public class SearchEmailsResult : BaseResult
{
    private SearchEmailsResult(bool success, IEnumerable<string> errors, IEnumerable<string> data) : base(success,
        errors)
    {
        Data = data;
    }

    public IEnumerable<string> Data { get; set; }

    public static SearchEmailsResult SuccessResult(IEnumerable<string> data)
    {
        return new SearchEmailsResult(true, [], data);
    }
}