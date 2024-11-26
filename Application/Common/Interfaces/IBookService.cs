using Application.Books.Results;
using Application.Common.Models;

namespace Application.Common.Interfaces;

public interface IBookService
{
    Task<AddBookResult> AddBookAsync(CreateBookModel model);
    Task<GetBookResult> GetBooksAsync();
}