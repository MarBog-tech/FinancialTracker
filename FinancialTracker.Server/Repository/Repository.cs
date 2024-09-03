using System.Linq.Expressions;
using FinancialTracker.Server.Data;
using Microsoft.EntityFrameworkCore;
using FinancialTracker.Server.Repository.IRepository;
namespace FinancialTracker.Server.Repository;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<Repository<T>> _logger;
    
    public Repository(ApplicationDbContext db, ILogger<Repository<T>> logger)
    {
        _db = db;
        _logger = logger;
    }
    public async Task Create(T entity)
    {
        try
        {
            await _db.Set<T>().AddAsync(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error while creating entity of type {typeof(T).Name}.");
            throw;
        }
    }

    public void Update(T entity)
    {
        try
        {
            _db.Set<T>().Update(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error while updating entity of type {typeof(T).Name}.");
            throw;
        }
    }

    public async Task<T?> Get(Expression<Func<T?, bool>> filter, string? includeProperties = null, bool tracked = false)
    {
        IQueryable<T?> query = tracked ? _db.Set<T>() : _db.Set<T>().AsNoTracking();

        query = query.Where(filter);
        
        if (!string.IsNullOrEmpty(includeProperties)) {
            query = includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Aggregate(query, (current, includeProp) => current.Include(includeProp.Trim()));
        }
        
        try
        {
            return await query.FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error while retrieving entity of type {typeof(T).Name}.");
            throw;
        }
    }

    public async Task<IEnumerable<T>> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null)
    {
        IQueryable<T> query = _db.Set<T>();
        if (filter != null)
            query = query.Where(filter);
        
        if (!string.IsNullOrEmpty(includeProperties)) {
            query = includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Aggregate(query, (current, includeProp) => current.Include(includeProp.Trim()));
        }
        
        try
        {
            return await query.AsNoTracking().ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error while retrieving all entities of type {typeof(T).Name}.");
            throw;
        }
    }

    public void Delete(T entity)
    {
        try
        {
            _db.Set<T>().Remove(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error while deleting entity of type {typeof(T).Name}.");
            throw;
        }
    }
    
    public async Task SaveAsync()
    {
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while saving changes to the database.");
            throw;
        }
    }
}