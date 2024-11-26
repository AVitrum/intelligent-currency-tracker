using Application.Common.Models;

namespace Application.Books.Results;

public class GetBookResult
{
    public bool Success { get; }
    public string? Message { get; }
    public IEnumerable<GetBookModel>? Books { get; }
    
    private GetBookResult(bool success, string? message, IEnumerable<GetBookModel>? books)
    {
        Success = success;
        Message = message;
        Books = books;
    }
    
    public static GetBookResult SuccessResult(IEnumerable<GetBookModel> books) => new(true, null, books);
    public static GetBookResult FailureResult(string message) => new(false, message, null);
}