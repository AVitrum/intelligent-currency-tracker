using Application.Common.Models;
using Domain.Common;

namespace Application.Common.Interfaces;

public interface IBookService
{
    Task<BaseResult> AddBookAsync(CreateBookModel model);
    Task<BaseResult> GetBooksAsync();
}