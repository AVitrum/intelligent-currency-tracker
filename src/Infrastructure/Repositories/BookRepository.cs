using Domain.Exceptions;

namespace Infrastructure.Repositories;

public class BookRepository : BaseRepository<Book>, IBookRepository
{
    private readonly ApplicationDbContext _context;
    
    public BookRepository(ApplicationDbContext context) : base(context, context.Books)
    {
        _context = context;
    }
    
    public override async Task<Book> GetByIdAsync(Guid id)
    {
        return await _context.Books.Include(b => b.Author).FirstOrDefaultAsync(b => b.Id == id) ??
               throw new EntityNotFoundException<Book>();
    }
}