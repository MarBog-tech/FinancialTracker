using System.ComponentModel.DataAnnotations;

namespace FinancialTracker.Client.Razor.Models.Entity;

public class CategoryDTO
{
    [Key]
    public Guid Id { get; set; }
    [Required] 
    public string Name { get; set; }
}