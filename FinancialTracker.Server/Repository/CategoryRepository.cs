using FinancialTracker.Server.Data;
using FinancialTracker.Server.Models;
using FinancialTracker.Server.Models.Entity;
using FinancialTracker.Server.Repository.IRepository;

namespace FinancialTracker.Server.Repository;

public class CategoryRepository(ApplicationDbContext db, ILogger<Repository<Category>> logger)
    : Repository<Category>(db, logger), ICategoryRepository
{
}