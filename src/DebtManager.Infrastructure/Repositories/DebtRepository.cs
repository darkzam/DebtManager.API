using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;
using DebtManager.Infrastructure.Contexts;

namespace DebtManager.Infrastructure.Repositories
{
    public class DebtRepository : BaseRepository<Debt>, IDebtRepository
    {
        public DebtRepository(DebtManagerContext DebtManagerContext) : base(DebtManagerContext)
        { }
    }
}
