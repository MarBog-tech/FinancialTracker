namespace FinancialTracker.Client.Models.VM;

public class TransactionGraphVM
{
    public List<string> Category { get; set; }
    public List<decimal> Amount { get; set; }
    public List<bool> IsIncome { get; set; }
    
}