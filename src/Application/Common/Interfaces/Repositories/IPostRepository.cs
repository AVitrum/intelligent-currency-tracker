using Domain.Entities;

namespace Application.Common.Interfaces.Repositories;

public interface IPostRepository : IBaseRepository<Post>
{
    Task<IEnumerable<FileLink>> GetAttachmentsByPostIdAsync(Guid postId);
    Task<IEnumerable<Post>> GetAllAsync(int page, int pageSize);
}