namespace Application.Common.Interfaces.Repositories;

public interface IBaseRepository<T>
{
    Task<bool> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<T> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
}