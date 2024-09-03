using System.Net;
using FinancialTracker.Server.Models;
using FinancialTracker.Server.Models.Entity;
using FinancialTracker.Server.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace FinancialTracker.Server.Controllers;

[Route("Api/CategoryAPI")]
[ApiController]
public class CategoryController: ControllerBase
{
    private readonly ICategoryRepository _category;

    public CategoryController(ICategoryRepository category)
    {
        _category = category;
    }
    
    [HttpGet]
    public async Task<ActionResult> GetCategories()
    {
        try
        {
            var categoryList = await _category.GetAll();
            if (categoryList == null || !categoryList.Any())
                return StatusCode(StatusCodes.Status404NotFound,new APIResponse
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    ErrorMessages = ["Category object not found."]
                });
            
            return StatusCode(StatusCodes.Status200OK,new APIResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = categoryList
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new APIResponse
            {
                StatusCode = HttpStatusCode.InternalServerError,
                IsSuccess = false,
                ErrorMessages = new List<string> { ex.ToString() }
            });
        }
    }
    
    [HttpGet("{id:guid}", Name = "GetCategory")]
    public async Task<ActionResult> GetCategory(Guid id)
    {
        try
        {
            var category = await _category.Get(x => x.Id == id);
            if (category == null)
                return StatusCode(StatusCodes.Status404NotFound,new APIResponse
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    ErrorMessages = ["Category object not found."]
                });

            return StatusCode(StatusCodes.Status200OK,new APIResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = category
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new APIResponse
            {
                StatusCode = HttpStatusCode.InternalServerError,
                IsSuccess = false,
                ErrorMessages = new List<string> { ex.ToString() }
            });
        }
    }
    
    [HttpPost]
    public async Task<ActionResult> CreateCategory([FromBody] Category category)
    {
        try
        {
            if (category == null)
                return StatusCode(StatusCodes.Status400BadRequest,new APIResponse
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = ["Invalid category object."]
                });
            
            await _category.Create(category);
            await _category.SaveAsync();

            return StatusCode(StatusCodes.Status201Created, new APIResponse
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.Created
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new APIResponse
            {
                IsSuccess = false,
                StatusCode = HttpStatusCode.InternalServerError,
                ErrorMessages = new List<string> { ex.ToString() }
            });
        }
    }

    [HttpDelete("{id:guid}", Name = "DeleteCategory")]
    public async Task<ActionResult> DeleteVillaNumber(Guid id)
    {
        try
        {
            var category = await _category.Get(x => x.Id == id);
            if (category == null)
                return StatusCode(StatusCodes.Status404NotFound,new APIResponse
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    ErrorMessages = ["Category object not found."]
                });

            _category.Delete(category);
            await _category.SaveAsync();

            return StatusCode(StatusCodes.Status200OK, new APIResponse
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new APIResponse
            {
                IsSuccess = false,
                StatusCode = HttpStatusCode.InternalServerError,
                ErrorMessages = new List<string> { ex.ToString() }
            });
        }
    }
    
    [HttpPut("{id:guid}", Name = "UpdateCategory")]
    public async Task<ActionResult> UpdateCategory(Guid id, [FromBody] Category category)
    {
        try
        {
            if (category == null || id != category.Id)
                return StatusCode(StatusCodes.Status400BadRequest,new APIResponse
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = ["Invalid category object."]
                });

            _category.Update(category);
            await _category.SaveAsync();

            return StatusCode(StatusCodes.Status200OK,new APIResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new APIResponse
            {
                IsSuccess = false,
                StatusCode = HttpStatusCode.InternalServerError,
                ErrorMessages = new List<string> { ex.ToString() }
            });
        }
    }
}