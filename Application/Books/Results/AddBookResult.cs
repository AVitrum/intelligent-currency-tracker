namespace Application.Books.Results;

public class AddBookResult
{
    public bool Success { get; }
    public string Message { get; }
    
    private AddBookResult(bool success, string message)
    {
        Success = success;
        Message = message;
    }
    
    public static AddBookResult SuccessResult() => new(true, "Book added successfully");
    public static AddBookResult FailureResult(string message) => new(false, message);
}