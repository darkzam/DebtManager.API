using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;
using DebtManager.Infrastructure.Contexts;

namespace DebtManager.Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(DebtManagerContext DebtManagerContext) : base(DebtManagerContext)
        { }
    }
}
