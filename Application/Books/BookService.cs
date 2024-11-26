using Application.Books.Results;
using Application.Common.Interfaces;
using Application.Common.Models;
using AutoMapper;
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
    
    public async Task<AddBookResult> AddBookAsync(CreateBookModel model)
    {
        var book = _mapper.Map<Book>(model);
        var result = await _bookRepository.AddAsync(book);
        
        return result ? AddBookResult.SuccessResult() : AddBookResult.FailureResult("Failed to add book");
    }

    public async Task<GetBookResult> GetBooksAsync()
    {
        var books = await _bookRepository.GetAllAsync();
        
        if (!books.Any()) return GetBookResult.FailureResult("Failed to get books");
        
        var booksModel = _mapper.Map<IEnumerable<GetBookModel>>(books);
        return GetBookResult.SuccessResult(booksModel);
    }
}