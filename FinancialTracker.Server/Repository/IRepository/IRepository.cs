using System.Linq.Expressions;

namespace FinancialTracker.Server.Repository.IRepository;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
    Task<T?> Get(Expression<Func<T?, bool>> filter, string? includeProperties = null, bool tracked = false);
    Task Create(T entity);
    void Delete(T entity);
    void Update(T entity);
    Task SaveAsync();
}