using Domain.Common;

namespace Application.Books.Results;

public class AddBookResult : BaseResult
{
    public string Message { get; }
    
    private AddBookResult(bool success, IEnumerable<string> errors, string message) : base(success, errors)
    {
        Message = message;
    }

    public static AddBookResult SuccessResult() => new(true, [], "Book added successfully");
    public static AddBookResult FailureResult(IEnumerable<string> errors) => new(false, errors, string.Empty);
}