using Domain.Common;

namespace Infrastructure.Identity.Results;

public class SearchEmailsResult : BaseResult
{
    public IEnumerable<string> Data { get; set; }
    
    private SearchEmailsResult(bool success, IEnumerable<string> errors, IEnumerable<string> data) : base(success, errors)
    {
        Data = data;
    }
    
    public static SearchEmailsResult SuccessResult(IEnumerable<string> data) => new(true, [], data);
}