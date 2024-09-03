using FinancialTracker.Server.Data;
using FinancialTracker.Server.Models;
using FinancialTracker.Server.Models.Entity;
using FinancialTracker.Server.Repository.IRepository;

namespace FinancialTracker.Server.Repository;

public class TransactionRepository(ApplicationDbContext db, ILogger<Repository<Transaction>> logger)
    : Repository<Transaction>(db, logger), ITransactionRepository
{
}