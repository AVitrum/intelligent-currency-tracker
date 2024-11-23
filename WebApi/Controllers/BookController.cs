using Application.Common.Interfaces;
using Application.Common.Models;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IBookRepository _bookRepository;

    public BookController(IBookRepository bookRepository, IMapper mapper)
    {
        _mapper = mapper;
        _bookRepository = bookRepository;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetBooks()
    {
        var books = await _bookRepository.GetAllAsync();
        return Ok(books);
    }
    
    [HttpPost]
    public async Task<IActionResult> AddBook(CreateBookModel model)
    {
        var book = _mapper.Map<Book>(model);
        
        await _bookRepository.AddAsync(book);
        return Ok("Book added successfully");
    }
}