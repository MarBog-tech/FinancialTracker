using System.ComponentModel.DataAnnotations;

namespace FinancialTracker.Client.Models.Entity;

public class Category
{
    public Guid Id { get; set; }
    [Required] 
    public string Name { get; set; }
}