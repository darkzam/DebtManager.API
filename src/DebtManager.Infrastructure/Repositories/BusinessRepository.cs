using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;
using DebtManager.Infrastructure.Contexts;

namespace DebtManager.Infrastructure.Repositories
{
    public class BusinessRepository : BaseRepository<Business>, IBusinessRepository
    {
        public BusinessRepository(DebtManagerContext DebtManagerContext) : base(DebtManagerContext)
        { }
    }
}
