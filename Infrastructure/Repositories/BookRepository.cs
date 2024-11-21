using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class BookRepository : BaseRepository<Book>, IBookRepository
{
    private readonly ApplicationDbContext _context;
    
    public BookRepository(ApplicationDbContext context) : base(context, context.Books)
    {
        _context = context;
    }
}