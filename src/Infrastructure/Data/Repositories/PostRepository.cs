using Application.Common.Interfaces.Repositories;
using Domain.Enums;
using Domain.Exceptions;

namespace Infrastructure.Data.Repositories;

public class PostRepository : BaseRepository<Post>, IPostRepository
{
    private readonly DbSet<Post> _posts;

    public PostRepository(ApplicationDbContext context) : base(context, context.Posts)
    {
        _posts = context.Posts;
    }

    public async Task<IEnumerable<FileLink>> GetAttachmentsByPostIdAsync(Guid postId)
    {
        Post post = await _posts
                        .Include(r => r.Attachments)
                        .FirstOrDefaultAsync(p => p.Id == postId)
                    ?? throw new EntityNotFoundException<Post>();

        return post.Attachments;
    }

    public async Task<IEnumerable<Post>> GetAllAsync(Language language, int page, int pageSize)
    {
        return await _posts
            .Where(p => p.Language == language)
            .OrderByDescending(p => p.TimeStamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}