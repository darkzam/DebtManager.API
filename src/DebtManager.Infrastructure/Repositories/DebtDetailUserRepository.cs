using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;
using DebtManager.Infrastructure.Contexts;

namespace DebtManager.Infrastructure.Repositories
{
    public class DebtDetailUserRepository : BaseRepository<DebtDetailUser>, IDebtDetailUserRepository
    {
        public DebtDetailUserRepository(DebtManagerContext DebtManagerContext) : base(DebtManagerContext)
        { }
    }
}
