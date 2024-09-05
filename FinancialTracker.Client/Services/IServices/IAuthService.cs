using FinancialTracker.Client.Models.Dto;

namespace FinancialTracker.Client.Services.IServices;

public interface IAuthService
{
    Task<T> LoginAsync<T>(LoginRequestDTO objToCreate);
    Task<T> RegisterAsync<T>(RegistrationRequestDTO objToCreate);
    Task<T> LogoutAsync<T>(TokenDTO obj);
    Task<T> GetProfileAsync<T>(Guid userId);
    Task<T> UpdateProfileAsync<T>(Guid userId, UserProfileDTO userProfileDto);
}