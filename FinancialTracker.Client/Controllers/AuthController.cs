using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FinancialTracker.Client.Models.Dto;
using FinancialTracker.Client.Models.Entity;
using FinancialTracker.Client.Models.Utility;
using FinancialTracker.Client.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace FinancialTracker.Client.Controllers
{ 
    [Authorize]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;
        public AuthController(IAuthService authService, ITokenProvider tokenProvider)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View(new LoginRequestDTO());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestDTO obj)
        {
            APIResponse response = await _authService.LoginAsync<APIResponse>(obj);
            if (response?.IsSuccess == true)
            {
                TokenDTO model = JsonConvert.DeserializeObject<TokenDTO>(response.Result.ToString());
                await SignInUserAsync(model);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("CustomError", response?.ErrorMessages?.FirstOrDefault());
            return View(obj);
        }
        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            ViewBag.RoleList = GetRoleList();
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegistrationRequestDTO obj)
        {
            obj.Role ??= SD.Customer;
            APIResponse result = await _authService.RegisterAsync<APIResponse>(obj);
            if (result?.IsSuccess == true)
            {
                return RedirectToAction("Login");
            }
            ViewBag.RoleList = GetRoleList();
            return View();
        }
        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            var token = _tokenProvider.GetToken();
            await _authService.LogoutAsync<APIResponse>(token);
            _tokenProvider.ClearToken();
            return RedirectToAction("Index", "Home");
        }
        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = Guid.Parse(claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            APIResponse response = await _authService.GetProfileAsync<APIResponse>(userId);
            if (response?.IsSuccess == true)
            {
                var userProfileDto = JsonConvert.DeserializeObject<UserProfileDTO>(response.Result.ToString());
                return View(userProfileDto);
            }
    
            return RedirectToAction("Login");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(UserProfileDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Отримати ID користувача
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            // Виклик API для оновлення профілю
            APIResponse response = await _authService.UpdateProfileAsync<APIResponse>(Guid.Parse(userId), model);
            if (response?.IsSuccess == true)
            {
                return RedirectToAction("Index", "Home");
            }
    
            ModelState.AddModelError("CustomError", response?.ErrorMessages?.FirstOrDefault());
            return View(model);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task SignInUserAsync(TokenDTO tokenDTO)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(tokenDTO.AccessToken);

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub).Value));
            identity.AddClaim(new Claim(ClaimTypes.Name, jwt.Claims.FirstOrDefault(u => u.Type == "unique_name")?.Value));
            identity.AddClaim(new Claim(ClaimTypes.Role, jwt.Claims.FirstOrDefault(u => u.Type == "role")?.Value));
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            _tokenProvider.SetToken(tokenDTO);
        }

        private static List<SelectListItem> GetRoleList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = SD.Admin, Value = SD.Admin },
                new SelectListItem { Text = SD.Customer, Value = SD.Customer },
            };
        }
    }
}