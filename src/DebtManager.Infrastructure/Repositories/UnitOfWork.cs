using DebtManager.Application.Common.Interfaces;
using DebtManager.Infrastructure.Contexts;

namespace DebtManager.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DebtManagerContext _DebtManagerContext;
        public UnitOfWork(DebtManagerContext DebtManagerContext)
        {
            _DebtManagerContext = DebtManagerContext ?? throw new ArgumentException(nameof(DebtManagerContext));
        }

        public IDebtRepository DebtRepository => new DebtRepository(_DebtManagerContext);
        public async Task<int> CompleteAsync()
        {
            return await _DebtManagerContext.SaveChangesAsync();
        }
    }
}
