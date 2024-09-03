using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace FinancialTracker.Server.Models.Entity;

public class User: IdentityUser<Guid>
{
    public virtual UserProfile UserProfile { get; set; }
    
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}