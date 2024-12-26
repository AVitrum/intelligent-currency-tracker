using Application.Common.Models;
using Domain.Common;

namespace Application.Books.Results;

public class GetBookResult : BaseResult
{
    public IEnumerable<GetBookModel> Books { get; }

    private GetBookResult(bool success, IEnumerable<string> errors, IEnumerable<GetBookModel> books) : base(success,
        errors)
    {
        Books = books;
    }

    public static GetBookResult SuccessResult(IEnumerable<GetBookModel> books) => new(true, [], books);
}