using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinancialTracker.Server.Models.Entity;

public class UserProfile
{
    [Key]
    [ForeignKey("User")]
    public Guid Id { get; set; }
    public virtual User User { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; } = DateTime.Today;


}