using FinancialTracker.Client.Models.Dto;

namespace FinancialTracker.Client.Services.IServices;

public interface IAuthService
{
    Task<T> LoginAsync<T>(LoginRequestDTO objToCreate);
    Task<T> RegisterAsync<T>(RegistrationRequestDTO objToCreate);
    Task<T> LogoutAsync<T>(TokenDTO obj);
}