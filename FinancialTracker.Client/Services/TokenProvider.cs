using FinancialTracker.Client.Models.Dto;
using FinancialTracker.Client.Models.Utility;
using FinancialTracker.Client.Services.IServices;

namespace FinancialTracker.Client.Services;

public class TokenProvider : ITokenProvider
{
    private readonly IHttpContextAccessor _contextAccessor;

    public TokenProvider(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public void ClearToken()
    {
        var response = _contextAccessor.HttpContext?.Response;
        response?.Cookies.Delete(SD.AccessToken);
        response?.Cookies.Delete(SD.RefreshToken);
    }

    public TokenDTO GetToken()
    {
        var request = _contextAccessor.HttpContext?.Request;
        if (request != null && request.Cookies.TryGetValue(SD.AccessToken, out var accessToken))
        {
            var refreshToken = request.Cookies.TryGetValue(SD.RefreshToken, out var existingRefreshToken) ? existingRefreshToken : null;
            return new TokenDTO { AccessToken = accessToken, RefreshToken = refreshToken };
        }
        return null;
    }

    public void SetToken(TokenDTO tokenDTO)
    {
        var response = _contextAccessor.HttpContext?.Response;
        if (response != null)
        {
            var cookieOptions = new CookieOptions { Expires = DateTime.UtcNow.AddDays(60) };
            response.Cookies.Append(SD.AccessToken, tokenDTO.AccessToken, cookieOptions);
            response.Cookies.Append(SD.RefreshToken, tokenDTO.RefreshToken, cookieOptions);
        }
    }
}