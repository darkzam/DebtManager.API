using DebtManager.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DebtManager.Infrastructure.Contexts
{
    public class DebtManagerContext : DbContext
    {
        public DebtManagerContext(DbContextOptions<DebtManagerContext> dbContextOptions) : base(dbContextOptions)
        { }

        public DbSet<Debt> Debts { get; set; }
    }
}
