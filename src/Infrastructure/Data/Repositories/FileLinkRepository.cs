using Application.Common.Interfaces.Repositories;

namespace Infrastructure.Data.Repositories;

public class FileLinkRepository : BaseRepository<FileLink>, IFileLinkRepository
{
    private readonly ApplicationDbContext _context;

    public FileLinkRepository(ApplicationDbContext context) : base(context, context.FileLinks)
    {
        _context = context;
    }
}