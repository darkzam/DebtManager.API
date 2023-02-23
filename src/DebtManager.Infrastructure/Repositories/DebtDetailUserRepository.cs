using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;
using DebtManager.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DebtManager.Infrastructure.Repositories
{
    public class DebtDetailUserRepository : BaseRepository<DebtDetailUser>, IDebtDetailUserRepository
    {
        public DebtDetailUserRepository(DebtManagerContext DebtManagerContext) : base(DebtManagerContext)
        { }

        public override async Task<IEnumerable<DebtDetailUser>> SearchBy(Expression<Func<DebtDetailUser, bool>> predicate)
        {
            return await _dbContext.Set<DebtDetailUser>()
                                   .Where(predicate)
                                   .Include(x => x.User)
                                   .Include(x => x.DebtDetail)
                                   .ThenInclude(x => x.Product)
                                   .ToListAsync();
        }
    }
}
