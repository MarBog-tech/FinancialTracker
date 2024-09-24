using System.Diagnostics;
using System.Security.Claims;
using FinancialTracker.Client.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using FinancialTracker.Client.Models;
using FinancialTracker.Client.Models.Dto;
using FinancialTracker.Client.Models.VM;
using FinancialTracker.Client.Services.IServices;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace FinancialTracker.Client.Controllers;

public class HomeController : Controller
{
    private readonly ITransactionService _transactionService;
    private readonly ICategoryService _categoryService;

    public HomeController(ITransactionService transactionService, ICategoryService categoryService)
    {
        _transactionService = transactionService;
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (User.Identity.IsAuthenticated)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = Guid.Parse(claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value);
            var response = await _transactionService.GetAllByUserAsync<APIResponse>(userId);
            if (response != null && response.IsSuccess)
            {
                return View(JsonConvert.DeserializeObject<List<TransactionIndexDTO>>(Convert.ToString(response.Result)));
            }
        }
        return View(new List<TransactionIndexDTO>());
    }
    
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        TransactionVM transactionVm = new TransactionVM();
        var response = await _categoryService.GetAllAsync<APIResponse>();
        
        if (response != null && response.IsSuccess)
        {
            transactionVm.CategoryList = JsonConvert.DeserializeObject<List<Category>>(Convert.ToString(response.Result))
                .Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });
        }
        return View(transactionVm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TransactionVM transactionVm)
    {
        APIResponse? response;
        if (ModelState.IsValid)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            transactionVm.Transaction.UserId = Guid.Parse(claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value);
            response = await _transactionService.CreateAsync<APIResponse>(transactionVm.Transaction);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Transaction created successfully";
                return RedirectToAction(nameof(Index));
            }
        }
        
        response = await _categoryService.GetAllAsync<APIResponse>();
        
        if (response != null && response.IsSuccess)
        {
            transactionVm.CategoryList = JsonConvert.DeserializeObject<List<Category>>(Convert.ToString(response.Result))
                .Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });
        }
        return View(transactionVm);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
     
        TransactionVM transactionVm = new TransactionVM();
       
        var response = await _transactionService.GetAsync<APIResponse>(id);
        if (response != null && response.IsSuccess)
        {
            transactionVm.Transaction = JsonConvert.DeserializeObject<TransactionDTO>(Convert.ToString(response.Result));
        }else
            return NotFound();
        
        
        response = await _categoryService.GetAllAsync<APIResponse>();
        if (response != null && response.IsSuccess)
        {
            transactionVm.CategoryList = JsonConvert.DeserializeObject<List<Category>>(Convert.ToString(response.Result))
                .Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });
        }
        return View(transactionVm);
        
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TransactionVM transactionVm)
    {
        APIResponse? response;
        if (ModelState.IsValid)
        {
            response = await _transactionService.UpdateAsync<APIResponse>(transactionVm.Transaction);
            if (response != null && response.IsSuccess)
            {
                return RedirectToAction(nameof(Index));
            }
        }
        response = await _categoryService.GetAllAsync<APIResponse>();
        if (response != null && response.IsSuccess)
        {
            transactionVm.CategoryList = JsonConvert.DeserializeObject<List<Category>>(Convert.ToString(response.Result))
                .Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });
        }
        return View(transactionVm);
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var response = await _transactionService.DeleteAsync<APIResponse>(id);
        if (response != null && response.IsSuccess)
        {
            TempData["success"] = "Transaction deleted successfully";
            return RedirectToAction(nameof(Index));
        }
        TempData["error"] = "An error occurred";
        return NotFound();
    }
    
    [HttpGet]
    public async Task<IActionResult> CreateGraphData()
    {
        if (User.Identity.IsAuthenticated)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = Guid.Parse(claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value);
            var response = await _transactionService.GetAllByUserAsync<APIResponse>(userId);
            if (response != null && response.IsSuccess)
            {
                var transactions = JsonConvert.DeserializeObject<List<TransactionIndexDTO>>(Convert.ToString(response.Result));

                var incomeData = transactions
                    .Where(t => t.IsIncome)
                    .GroupBy(t => t.Category)
                    .Select(group => new
                    {
                        Category = group.Key,
                        Amount = group.Sum(t => t.Amount),
                        IsIncome = true
                    })
                    .ToList();

                // Групуємо і сумуємо дані для витрат
                var expenseData = transactions
                    .Where(t => !t.IsIncome)
                    .GroupBy(t => t.Category)
                    .Select(group => new
                    {
                        Category = group.Key,
                        Amount = group.Sum(t => t.Amount),
                        IsIncome = false
                    })
                    .ToList();

                var data = new
                {
                    incomeData,
                    expenseData
                };

                return Json(new { success = true, data });
            }
        }
        return Json(new { success = false });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }
}