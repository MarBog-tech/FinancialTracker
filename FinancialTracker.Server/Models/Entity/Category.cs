using System.ComponentModel.DataAnnotations;

namespace FinancialTracker.Server.Models.Entity;

public class Category
{
    [Key]
    public Guid Id { get; set; }
    [Required] 
    public string Name { get; set; }
}