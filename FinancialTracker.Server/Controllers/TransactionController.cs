using System.Net;
using FinancialTracker.Server.Models;
using FinancialTracker.Server.Models.Entity;
using FinancialTracker.Server.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace FinancialTracker.Server.Controllers;

[Route("Api/TransactionAPI")]
[ApiController]
public class TransactionController : ControllerBase
{
    private readonly ITransactionRepository _transactionRepo;

    public TransactionController(ITransactionRepository transactionRepo)
    {
        _transactionRepo = transactionRepo;
    }

    [HttpGet]
    public async Task<ActionResult> GetTransactions()
    {
        try
        {
            var transactions = await _transactionRepo.GetAll();
            if (transactions == null || !transactions.Any())
            {
                return StatusCode(StatusCodes.Status404NotFound, new APIResponse
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    ErrorMessages = ["No transactions found."]
                });
            }

            return StatusCode(StatusCodes.Status200OK, new APIResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = transactions
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
    
    [HttpGet("User/{userId:guid}")]
    public async Task<ActionResult> GetTransactionsByUserId(Guid userId)
    {
        try
        {
            var transactions = await _transactionRepo.GetAll(t => t.UserId == userId);
            if (transactions == null || !transactions.Any())
            {
                return StatusCode(StatusCodes.Status404NotFound, new APIResponse
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    ErrorMessages = ["No transactions found for the specified user."]
                });
            }

            return StatusCode(StatusCodes.Status200OK, new APIResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = transactions
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

    [HttpGet("{id:guid}", Name = "GetTransaction")]
    public async Task<ActionResult> GetTransaction(Guid id)
    {
        try
        {
            var transaction = await _transactionRepo.Get(x => x.Id == id);
            if (transaction == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new APIResponse
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    ErrorMessages = ["Transaction not found."]
                });
            }

            return StatusCode(StatusCodes.Status200OK, new APIResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = transaction
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
    public async Task<ActionResult> CreateTransaction([FromBody] Transaction transaction)
    {
        try
        {
            if (transaction == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new APIResponse
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = ["Invalid transaction object."]
                });
            }

            await _transactionRepo.Create(transaction);
            await _transactionRepo.SaveAsync();

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

    [HttpDelete("{id:guid}", Name = "DeleteTransaction")]
    public async Task<ActionResult> DeleteTransaction(Guid id)
    {
        try
        {
            var transaction = await _transactionRepo.Get(x => x.Id == id);
            if (transaction == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new APIResponse
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    ErrorMessages = ["Transaction not found."]
                });
            }

            _transactionRepo.Delete(transaction);
            await _transactionRepo.SaveAsync();

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

    [HttpPut("{id:guid}", Name = "UpdateTransaction")]
    public async Task<ActionResult> UpdateTransaction(Guid id, [FromBody] Transaction transaction)
    {
        try
        {
            if (transaction == null || id != transaction.Id)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new APIResponse
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = ["Invalid transaction object."]
                });
            }

            _transactionRepo.Update(transaction);
            await _transactionRepo.SaveAsync();

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
}