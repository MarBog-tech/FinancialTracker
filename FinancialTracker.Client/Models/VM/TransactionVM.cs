using FinancialTracker.Client.Models.Dto;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinancialTracker.Client.Models.VM;

public class TransactionVM
{
    public  TransactionDTO Transaction { get; set; }
    [ValidateNever]
    public IEnumerable<SelectListItem> CategoryList { get; set; }
}