using FinancialTracker.Client.Models.Dto;
using FinancialTracker.Client.Models.Entity;
using FinancialTracker.Client.Models.Utility;
using FinancialTracker.Client.Services.IServices;

namespace FinancialTracker.Client.Services;

public class AuthService : IAuthService
{
    private readonly IHttpClientFactory _clientFactory;
    private string _url;
    private readonly IBaseService _baseService;
    public AuthService(IHttpClientFactory clientFactory, IConfiguration configuration, IBaseService baseService) 
    {
        _baseService = baseService;
        _clientFactory = clientFactory;
        _url = configuration.GetValue<string>("ServiceUrls:API");

    }

    public async Task<T> LoginAsync<T>(LoginRequestDTO obj)
    {
        return await _baseService.SendAsync<T>(new APIRequest()
        {
            ApiType = SD.ApiType.POST,
            Data = obj,
            Url = _url + $"/api/UsersAuth/login"
        },withBearer:false);
    }

    public async Task<T> RegisterAsync<T>(RegistrationRequestDTO obj)
    {
        return await _baseService.SendAsync<T>(new APIRequest()
        {
            ApiType = SD.ApiType.POST,
            Data = obj,
            Url = _url + $"/api/UsersAuth/register"
        }, withBearer: false);
    }

    public async Task<T> LogoutAsync<T>(TokenDTO obj)
    {
        return await _baseService.SendAsync<T>(new APIRequest()
        {
            ApiType = SD.ApiType.POST,
            Data = obj,
            Url = _url + $"/api/UsersAuth/revoke"
        });
    }
}