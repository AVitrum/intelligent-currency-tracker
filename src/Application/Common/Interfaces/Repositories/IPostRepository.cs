using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Interfaces.Repositories;

public interface IPostRepository : IBaseRepository<Post>
{
    Task<IEnumerable<FileLink>> GetAttachmentsByPostIdAsync(Guid postId);
    Task<IEnumerable<Post>> GetAllAsync(Language language, int page, int pageSize);
}