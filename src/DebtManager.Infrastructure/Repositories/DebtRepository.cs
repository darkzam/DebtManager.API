using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;
using DebtManager.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DebtManager.Infrastructure.Repositories
{
    public class DebtRepository : BaseRepository<Debt>, IDebtRepository
    {
        public DebtRepository(DebtManagerContext DebtManagerContext) : base(DebtManagerContext)
        { }

        public override async Task<IEnumerable<Debt>> SearchBy(Expression<Func<Debt, bool>> predicate)
        {
            return await _dbContext.Set<Debt>()
                                   .Where(predicate)
                                   .Include(x => x.Business)
                                   .Include(x => x.Host)
                                   .ToListAsync();
        }

        public override async Task<IEnumerable<Debt>> GetAll()
        {
            return await _dbContext.Set<Debt>()
                                   .Include(x => x.Business)
                                   .Include(x => x.Host)
                                   .ToListAsync();
        }
    }
}
