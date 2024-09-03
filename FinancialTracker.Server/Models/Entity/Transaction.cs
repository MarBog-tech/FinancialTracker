using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinancialTracker.Server.Models.Entity;

public class Transaction
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    [Required]
    public bool IsIncome { get; set; }

    [ForeignKey("User")]
    public Guid UserId { get; set; }
    public virtual User User { get; set; }

    [ForeignKey("Category")]
    public Guid CategoryId { get; set; }
    public virtual Category Category { get; set; }
}