using FinancialTracker.Client.Models.Entity;
using FinancialTracker.Client.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FinancialTracker.Client.Controllers;

[Authorize]
public class CategoryController : Controller
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }
    
    public async Task<IActionResult> IndexCategory()
    {
        List<Category> list = new();
        var response = await _categoryService.GetAllAsync<APIResponse>();
        if (response != null && response.IsSuccess)
        {
            list = JsonConvert.DeserializeObject<List<Category>>(Convert.ToString(response.Result));
        }
        return View(list);
    }

    public async Task<IActionResult> CreateCategory()
    {
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(Category model)
    {
        if (ModelState.IsValid)
        {
            var response = await _categoryService.CreateAsync<APIResponse>(model);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Category created successfully";
                return RedirectToAction(nameof(IndexCategory));
            }
        }
        TempData["error"] = "An error occurred";
        return View(model);
    }

    public async Task<IActionResult> UpdateCategory(Guid categoryId)
    {
        var response = await _categoryService.GetAsync<APIResponse>(categoryId);
        if (response != null && response.IsSuccess)
        {
            Category model = JsonConvert.DeserializeObject<Category>(Convert.ToString(response.Result));
            return View(model);
        }
        return NotFound();
    }
    
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCategory(Category model)
    {
        if (ModelState.IsValid)
        {
            var response = await _categoryService.UpdateAsync<APIResponse>(model);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Category updated successfully";
                return RedirectToAction(nameof(IndexCategory));        
            }
        }
        TempData["error"] = "An error occurred";
        return View(model);
    }
    
    public async Task<IActionResult> DeleteCategory(Guid categoryId)
    {
        var response = await _categoryService.DeleteAsync<APIResponse>(categoryId);
        if (response != null && response.IsSuccess)
        {
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction(nameof(IndexCategory));
        }
        TempData["error"] = "An error occurred";
        return NotFound();
    }
}