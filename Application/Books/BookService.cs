using Application.Books.Results;
using Application.Common.Interfaces;
using Application.Common.Models;
using AutoMapper;
using Domain.Common;
using Domain.Entities;

namespace Application.Books;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly IMapper _mapper;

    public BookService(IBookRepository bookRepository, IMapper mapper)
    {
        _bookRepository = bookRepository;
        _mapper = mapper;
    }
    
    public async Task<BaseResult> AddBookAsync(CreateBookModel model)
    {
        var book = _mapper.Map<Book>(model);
        var result = await _bookRepository.AddAsync(book);
        
        return result ? BaseResult.SuccessResult() : BaseResult.FailureResult(["Failed to add book"]);
    }

    public async Task<BaseResult> GetBooksAsync()
    {
        var books = await _bookRepository.GetAllAsync();
        
        if (!books.Any()) return BaseResult.FailureResult(["Failed to get books"]);
        
        var booksModel = _mapper.Map<IEnumerable<GetBookModel>>(books);
        return GetBookResult.SuccessResult(booksModel);
    }
}