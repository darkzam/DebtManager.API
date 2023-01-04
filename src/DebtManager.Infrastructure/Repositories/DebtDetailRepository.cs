using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;
using DebtManager.Infrastructure.Contexts;

namespace DebtManager.Infrastructure.Repositories
{
    public class DebtDetailRepository : BaseRepository<DebtDetail>, IDebtDetailRepository
    {
        public DebtDetailRepository(DebtManagerContext DebtManagerContext) : base(DebtManagerContext)
        { }
    }
}
