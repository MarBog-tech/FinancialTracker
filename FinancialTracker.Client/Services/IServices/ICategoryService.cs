using FinancialTracker.Client.Models.Entity;

namespace FinancialTracker.Client.Services.IServices;

public interface ICategoryService
{
    Task<T> GetAllAsync<T>();
    Task<T> GetAsync<T>(Guid id);
    Task<T> CreateAsync<T>(Category dto);
    Task<T> UpdateAsync<T>(Category dto);
    Task<T> DeleteAsync<T>(Guid id);
}