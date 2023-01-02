using DebtManager.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DebtManager.Infrastructure.Contexts
{
    public class DebtManagerContext : DbContext
    {
        public DebtManagerContext(DbContextOptions<DebtManagerContext> dbContextOptions) : base(dbContextOptions)
        { }

        public DbSet<Debt> Debt { get; set; }
        public DbSet<DebtDetail> DebtDetail { get; set; }
        public DbSet<DebtDetailUser> DebtDetailUser { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Business> Business { get; set; }
        public DbSet<Price> Price { get; set; }
    }
}
