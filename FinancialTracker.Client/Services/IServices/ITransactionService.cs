using FinancialTracker.Client.Models.Entity;

namespace FinancialTracker.Client.Services.IServices;

public interface ITransactionService
{
    Task<T> GetAllAsync<T>();
    Task<T> GetAsync<T>(Guid id);
    Task<T> CreateAsync<T>(TransactionDTO dto);
    Task<T> UpdateAsync<T>(TransactionDTO dto);
    Task<T> DeleteAsync<T>(Guid id);
}