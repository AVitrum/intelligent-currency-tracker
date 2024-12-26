using Application.Books.Results;
using Domain.Common;

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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddBook(CreateBookModel model)
    {
        BaseResult result = await _bookService.AddBookAsync(model);
        
        if (result.Success) return Ok("Book added successfully");
        
        return BadRequest(result.Errors);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetBooks()
    {
        BaseResult result = await _bookService.GetBooksAsync();
        
        if (result is not GetBookResult getBooksResult) return BadRequest("Failed to get books");
        
        return Ok(getBooksResult.Books);
    }
}