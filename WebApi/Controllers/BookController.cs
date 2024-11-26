using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookController : ControllerBase
{   
    private readonly IBookService _bookService;
    public BookController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpPost]
    public async Task<IActionResult> AddBook(CreateBookModel model)
    {
        var result = await _bookService.AddBookAsync(model);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetBooks()
    {
        var result = await _bookService.GetBooksAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }
}