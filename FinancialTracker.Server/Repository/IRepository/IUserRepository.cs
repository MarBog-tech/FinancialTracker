using FinancialTracker.Server.Models.Dto;

namespace FinancialTracker.Server.Repository.IRepository;

public interface IUserRepository
{
    Task<TokenDTO> Login(LoginRequestDTO loginRequestDTO);
    Task<UserDTO> Register(RegistrationRequestDTO registrationRequestDto);
    Task<TokenDTO> RefreshAccessToken(TokenDTO tokenDTO);
    Task RevokeRefreshToken(TokenDTO tokenDTO);
    Task<UserProfileDTO?> GetUserProfile(Guid userId);
    Task<UserProfileDTO> UpdateUserProfile(Guid userId, UserProfileDTO userProfileDto);
}