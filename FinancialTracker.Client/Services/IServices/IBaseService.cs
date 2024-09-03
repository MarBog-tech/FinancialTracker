using FinancialTracker.Client.Models.Entity;

namespace FinancialTracker.Client.Services.IServices;

public interface IBaseService
{
    Task<T> SendAsync<T>(APIRequest apiRequest, bool withBearer = true);
}