using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using FinancialTracker.Server.Data;
using FinancialTracker.Server.Models.Dto;
using FinancialTracker.Server.Models.Entity;
using FinancialTracker.Server.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FinancialTracker.Server.Repository;

public class UserRepository: IUserRepository
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly string? _secretKey;
    private readonly IMapper _mapper;


    public UserRepository(ApplicationDbContext db, IConfiguration configuration, 
        UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager, 
        IMapper mapper)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
        _mapper = mapper;
        _secretKey = configuration.GetValue<string>("ApiSettings:Secret") 
                     ?? throw new ArgumentNullException("Secret key cannot be null.");
    }
    
    public async Task<TokenDTO> Login(LoginRequestDTO login)
    {
        var user = await _userManager.FindByEmailAsync(login.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, login.Password))
            throw new InvalidOperationException("Email or password is incorrect"); ;
            
        
        var jwtTokenId = $"JTI{Guid.NewGuid()}";
        var accessToken = await GetAccessToken(user,jwtTokenId);
        var refreshToken = await CreateNewRefreshToken(user.Id.ToString(), jwtTokenId);
        return  new TokenDTO{
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<UserDTO?> Register(RegistrationRequestDTO registration)
    {
        if (await _userManager.FindByNameAsync(registration.UserName) != null) 
            throw new InvalidOperationException("User already exists with the given username."); ;
        
        User user = new()
        {
            UserName = registration.UserName,
            Email=registration.Email
        };
        
        //При створенні користувача не враховує спец символи у паролі
        var result = await _userManager.CreateAsync(user, registration.Password);
        if (!result.Succeeded) 
            throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        
        if (!await _roleManager.RoleExistsAsync(registration.Role))
        {
            await _roleManager.CreateAsync(new IdentityRole<Guid>(registration.Role));
        }

        await _userManager.AddToRoleAsync(user, registration.Role);
        
        // var userToReturn = await _db.Users.FirstOrDefaultAsync(u => u.UserName == registration.UserName);
        UserProfile userProfile = new UserProfile { Id = user.Id, Email = user.Email};
        await _db.UserProfile.AddAsync(userProfile);
        await _db.SaveChangesAsync();
        
        return _mapper.Map<UserDTO>(user);
    }
    
    private async Task<string> GetAccessToken(User user, string jwtTokenId)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                    new Claim(JwtRegisteredClaimNames.Jti, jwtTokenId),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(1),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    public async Task<TokenDTO> RefreshAccessToken(TokenDTO tokenDTO)
    {
        var existingRefreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(u => u.Refresh_Token == tokenDTO.RefreshToken);
        if (existingRefreshToken == null || !await IsTokenValid(existingRefreshToken, tokenDTO.AccessToken))
            throw new SecurityTokenException("Invalid or non-existent refresh token provided.");

        if (!existingRefreshToken.IsValid || existingRefreshToken.ExpiresAt < DateTime.UtcNow)
        {
            await MarkTokenAsInvalid(existingRefreshToken);
            throw new SecurityTokenExpiredException("The refresh token has expired.");
        }

        var newRefreshToken = await CreateNewRefreshToken(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);
        await MarkTokenAsInvalid(existingRefreshToken);

        var applicationUser = await _db.Users.FindAsync(existingRefreshToken.UserId);
        if (applicationUser == null) 
            throw new InvalidOperationException("User associated with the refresh token does not exist.");

        var newAccessToken = await GetAccessToken(applicationUser, existingRefreshToken.JwtTokenId);

        return new TokenDTO
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }

    public async Task RevokeRefreshToken(TokenDTO tokenDTO)
    {
        var existingRefreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(_ => _.Refresh_Token == tokenDTO.RefreshToken);
        
        if (existingRefreshToken == null || !await IsTokenValid(existingRefreshToken, tokenDTO.AccessToken))
        {
            return;
        }

        await MarkAllTokensInChainAsInvalid(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);
    }

    private async Task<string> CreateNewRefreshToken(string userId, string tokenId)
    {
        RefreshToken refreshToken = new()
        {
            IsValid = true,
            UserId = userId,
            JwtTokenId = tokenId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            Refresh_Token = $"{Guid.NewGuid()}-{Guid.NewGuid()}"
        };

        await _db.RefreshTokens.AddAsync(refreshToken);
        await _db.SaveChangesAsync();
        
        return refreshToken.Refresh_Token;
    }

    private bool GetAccessTokenData(string accessToken, string expectedUserId, string expectedTokenId)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.ReadJwtToken(accessToken);
            var jwtTokenId = jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Jti)?.Value;
            var userId = jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub)?.Value;

            return userId == expectedUserId && jwtTokenId == expectedTokenId;
        }
        catch(Exception ex)
        {
            throw new SecurityTokenException("Invalid token.", ex);
        }
    }
    
    private async Task MarkAllTokensInChainAsInvalid(string userId, string tokenId)
    {
        try
        {
            await _db.RefreshTokens
                .Where(u => u.UserId == userId && u.JwtTokenId == tokenId)
                .ExecuteUpdateAsync(u => u.SetProperty(refreshToken => refreshToken.IsValid, false));
        }
        catch (Exception ex)
        {
            throw new DbUpdateException("Failed to invalidate tokens.", ex);
        }
    }
    
    private Task MarkTokenAsInvalid(RefreshToken refreshToken)
    {
        refreshToken.IsValid = false; 
        return _db.SaveChangesAsync();
    }
    
    private async Task<bool> IsTokenValid(RefreshToken refreshToken, string accessToken)
    {
        var isValid = GetAccessTokenData(accessToken, refreshToken.UserId, refreshToken.JwtTokenId);
        if (!isValid)
            await MarkTokenAsInvalid(refreshToken);
        return isValid;
    }
    
    public async Task<UserProfileDTO?> GetUserProfile(Guid userId)
    {
        var userProfile = await _db.UserProfile.FirstOrDefaultAsync(u => u.Id == userId);
        if (userProfile == null) return null;
        var profile = _mapper.Map<UserProfileDTO>(userProfile);
        return profile;
    }
    
    public async Task<UserProfileDTO> UpdateUserProfile(Guid userId, UserProfileDTO userProfileDto)
    {
        var userProfile = await _db.UserProfile.FindAsync(userId);
        if (userProfile == null)
            throw new InvalidOperationException("User profile not found.");

        userProfile.Name = userProfileDto.Name;
        userProfile.Surname = userProfileDto.Surname;
        userProfile.PhoneNumber = userProfileDto.PhoneNumber;
        userProfile.Email = userProfileDto.Email;

        _db.UserProfile.Update(userProfile);
        await _db.SaveChangesAsync();

        return _mapper.Map<UserProfileDTO>(userProfile);
    }
}