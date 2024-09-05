using System.Net;
using FinancialTracker.Server.Models.Dto;
using FinancialTracker.Server.Models.Entity;
using FinancialTracker.Server.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace FinancialTracker.Server.Controllers;

[Route("Api/UsersAuth")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepo;
    public UsersController(IUserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
    {
        try
        {
            var tokenDto = await _userRepo.Login(model);
            return StatusCode(StatusCodes.Status200OK,new APIResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = tokenDto
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status400BadRequest,new APIResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                IsSuccess = false,
                ErrorMessages = new List<string> { ex.ToString() }
            });
        }
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] RegistrationRequestDTO model)
    {
        try
        {
            await _userRepo.Register(model);
        
            return StatusCode(StatusCodes.Status201Created,new APIResponse
            {
                StatusCode = HttpStatusCode.Created,
                IsSuccess = true
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status400BadRequest,new APIResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                IsSuccess = false,
                ErrorMessages = new List<string> { ex.ToString() }
            });
        }
        
       
    }

    [HttpPost("Refresh")]
    public async Task<IActionResult> GetNewTokenFromRefreshToken([FromBody] TokenDTO tokenDTO)
    {
        if (!ModelState.IsValid)
            return StatusCode(StatusCodes.Status400BadRequest,new APIResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                IsSuccess = false,
                ErrorMessages = ["Invalid Input token"]
            });
        try
        {
            var tokenDTOResponse = await _userRepo.RefreshAccessToken(tokenDTO);
            
            return StatusCode(StatusCodes.Status200OK,new APIResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = tokenDTOResponse
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status400BadRequest,new APIResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                IsSuccess = false,
                ErrorMessages = new List<string> { ex.ToString() }
            });
        }
    }
    
    [HttpPost("Revoke")]
    public async Task<IActionResult> RevokeRefreshToken([FromBody] TokenDTO tokenDTO)
    {
        if (!ModelState.IsValid)
            return StatusCode(StatusCodes.Status400BadRequest,new APIResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                IsSuccess = false,
                ErrorMessages = ["Invalid Input token"]
            });

        await _userRepo.RevokeRefreshToken(tokenDTO);
        return StatusCode(StatusCodes.Status200OK,new APIResponse
        {
            StatusCode = HttpStatusCode.OK,
            IsSuccess = true
        });    
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserProfile(Guid id)
    {
        var userProfile = await _userRepo.GetUserProfile(id);
        if (userProfile == null)
        {
            return NotFound(new APIResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                IsSuccess = false,
                ErrorMessages = new List<string> { "User profile not found." }
            });
        }
    
        return Ok(new APIResponse
        {
            StatusCode = HttpStatusCode.OK,
            IsSuccess = true,
            Result = userProfile
        });
    }
    
    [HttpPut("UpdateProfile/{id:guid}", Name ="UpdateProfile")]
    public async Task<IActionResult> UpdateUserProfile(Guid id, [FromBody] UserProfileDTO model)
    {
        if (!ModelState.IsValid)
            return BadRequest(new APIResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                IsSuccess = false,
                ErrorMessages = new List<string> { "Invalid input data" }
            });

        try
        {
            var updatedProfile = await _userRepo.UpdateUserProfile(id, model);
            return StatusCode(StatusCodes.Status200OK,new APIResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = updatedProfile
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status400BadRequest, new APIResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                IsSuccess = false,
                ErrorMessages = new List<string> { ex.Message }
            });
        }
    }
}