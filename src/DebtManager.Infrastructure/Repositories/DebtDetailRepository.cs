using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;
using DebtManager.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DebtManager.Infrastructure.Repositories
{
    public class DebtDetailRepository : BaseRepository<DebtDetail>, IDebtDetailRepository
    {
        public DebtDetailRepository(DebtManagerContext DebtManagerContext) : base(DebtManagerContext)
        { }

        public override async Task<IEnumerable<DebtDetail>> SearchBy(Expression<Func<DebtDetail, bool>> predicate)
        {
            return await _dbContext.Set<DebtDetail>()
                                   .Where(predicate)
                                   .Include(x => x.Product)
                                   .Include(x => x.Debt)
                                   .ToListAsync();
        }
    }
}
