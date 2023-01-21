using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;
using DebtManager.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace DebtManager.Infrastructure.Repositories
{
    public class PriceRepository : BaseRepository<Price>, IPriceRepository
    {
        public PriceRepository(DebtManagerContext DebtManagerContext) : base(DebtManagerContext)
        { }

        public override async Task<IEnumerable<Price>> GetAll()
        {
            return await _dbContext.Set<Price>()
                                   .Include(x => x.Product)
                                   .ToListAsync();
        }
    }
}
