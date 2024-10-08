using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using FinancialTracker.Client.Models.Dto;
using FinancialTracker.Client.Models.Entity;
using FinancialTracker.Client.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Newtonsoft.Json;

namespace FinancialTracker.Client.Services;

public class BaseService : IBaseService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenProvider _tokenProvider;
        private readonly IApiMessageRequestBuilder _apiMessageRequestBuilder;
        private readonly IHttpContextAccessor _httpContextAccessor;
        

        public BaseService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider, 
            IConfiguration configuration, IHttpContextAccessor httpContextAccessor, 
            IApiMessageRequestBuilder apiMessageRequestBuilder)
        {
            _httpClientFactory = httpClientFactory;
            _tokenProvider = tokenProvider;
            _httpContextAccessor = httpContextAccessor;
            _apiMessageRequestBuilder = apiMessageRequestBuilder;
            ApiUrl = configuration["backend_url"] ?? configuration.GetValue<string>("ServiceUrls:API");
        }
        public string ApiUrl { get; }
        
        public async Task<T> SendAsync<T>(APIRequest apiRequest, bool withBearer = true)
        {
            try
            {
                try
                {
                    var client = _httpClientFactory.CreateClient("API");
                    var httpRequestMessage = _apiMessageRequestBuilder.Build(apiRequest);

                    HttpResponseMessage httpResponseMessage = await SendWithRefreshTokenAsync(client, httpRequestMessage, withBearer);
                    var finalApiResponse = await ProcessResponseAsync(httpResponseMessage);

                    return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(finalApiResponse));
                }
                catch (AuthException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    var dto = new APIResponse
                    {
                        ErrorMessages = new List<string> { ex.Message },
                        IsSuccess = false
                    };

                    return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(dto));
                }

            }
            catch (AuthException)
            {
                throw;
            }
            catch (Exception e)
            {
                var dto = new APIResponse
                {
                    ErrorMessages = new List<string> { Convert.ToString(e.Message) },
                    IsSuccess = false
                };
                var res = JsonConvert.SerializeObject(dto);
                var APIResponse = JsonConvert.DeserializeObject<T>(res);
                return APIResponse;
            }
            
        }

        private async Task<HttpResponseMessage> SendWithRefreshTokenAsync(HttpClient httpClient,
            HttpRequestMessage httpRequestMessage, bool withBearer = true)
        {
            if (withBearer)
            {
                TokenDTO tokenDTO = _tokenProvider.GetToken();
                if (tokenDTO != null && !string.IsNullOrEmpty(tokenDTO.AccessToken))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenDTO.AccessToken);
                }

                try
                {
                    var response = await httpClient.SendAsync(httpRequestMessage);
                    if (response.IsSuccessStatusCode)
                        return response;

                    if (!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        await InvokeRefreshTokenEndpoint(httpClient, tokenDTO.AccessToken, tokenDTO.RefreshToken);
                        return await httpClient.SendAsync(httpRequestMessage);
                    }
                    return response;

                }
                catch (HttpRequestException httpRequestException)
                {
                    if (httpRequestException.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        await InvokeRefreshTokenEndpoint(httpClient, tokenDTO.AccessToken, tokenDTO.RefreshToken);
                        return await httpClient.SendAsync(httpRequestMessage);
                    }
                    throw;
                }
            }
            return await httpClient.SendAsync(httpRequestMessage);
        }

        private async Task InvokeRefreshTokenEndpoint(HttpClient httpClient, string existingAccessToken, string existingRefreshToken)
        {
            var message = new HttpRequestMessage
            {
                Headers = { { "Accept", "application/json" } },
                RequestUri = new Uri($"{ApiUrl}/api/UsersAuth/refresh"),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(new TokenDTO
                {
                    AccessToken = existingAccessToken,
                    RefreshToken = existingRefreshToken
                }), Encoding.UTF8, "application/json")
            };

            var response = await httpClient.SendAsync(message);
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<APIResponse>(content);

            if (apiResponse?.IsSuccess != true)
            {
                await _httpContextAccessor.HttpContext.SignOutAsync();
                _tokenProvider.ClearToken();
                throw new AuthException();
            }
            else
            {
                var tokenDataStr = JsonConvert.SerializeObject(apiResponse.Result);
                var tokenDto = JsonConvert.DeserializeObject<TokenDTO>(tokenDataStr);

                if (tokenDto != null && !string.IsNullOrEmpty(tokenDto.AccessToken))
                {
                    await SignInWithNewTokens(tokenDto);
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenDto.AccessToken);
                }
            }
        }

        private async Task SignInWithNewTokens(TokenDTO tokenDTO)
        {
            // Розшифрування отриманого JWT-токена
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(tokenDTO.AccessToken);

            // Створення та налаштування ідентичності для нового користувача
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.Name, jwt.Claims.FirstOrDefault(u => u.Type == "unique_name").Value));
            identity.AddClaim(new Claim(ClaimTypes.Role, jwt.Claims.FirstOrDefault(u => u.Type == "role").Value));
            var principal = new ClaimsPrincipal(identity);

            // Увійти в систему з новою ідентичністю користувача
            await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Оновити токени
            _tokenProvider.SetToken(tokenDTO);
        }
        
        private async Task<APIResponse> ProcessResponseAsync(HttpResponseMessage httpResponseMessage)
        {
            var finalApiResponse = new APIResponse { IsSuccess = false };

            try
            {
                switch (httpResponseMessage.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        finalApiResponse.ErrorMessages = new List<string> { "Not Found" };
                        break;
                    case HttpStatusCode.Forbidden:
                        finalApiResponse.ErrorMessages = new List<string> { "Access Denied" };
                        break;
                    case HttpStatusCode.Unauthorized:
                        finalApiResponse.ErrorMessages = new List<string> { "Unauthorized" };
                        break;
                    case HttpStatusCode.InternalServerError:
                        finalApiResponse.ErrorMessages = new List<string> { "Internal Server Error" };
                        break;
                    default:
                        var apiContent = await httpResponseMessage.Content.ReadAsStringAsync();
                        finalApiResponse.IsSuccess = true;
                        finalApiResponse = JsonConvert.DeserializeObject<APIResponse>(apiContent);
                        break;
                }
            }
            catch (Exception e)
            {
                finalApiResponse.ErrorMessages = new List<string> { "Error Encountered", e.Message };
            }

            return finalApiResponse;
        }
    }