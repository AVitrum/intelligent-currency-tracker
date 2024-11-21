using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookController : ControllerBase
{
    private readonly IBookRepository _bookRepository;

    public BookController(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetBooks()
    {
        var books = await _bookRepository.GetAllAsync();
        return Ok(books);
    }
    
    [HttpPost]
    public async Task<IActionResult> AddBook(Book book)
    {
        await _bookRepository.AddAsync(book);
        return Ok();
    }
}