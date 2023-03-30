using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;
using DebtManager.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DebtManager.Infrastructure.Repositories
{
    public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(DebtManagerContext DebtManagerContext) : base(DebtManagerContext)
        { }

        public override async Task<IEnumerable<Payment>> SearchBy(Expression<Func<Payment, bool>> predicate)
        {
            return await _dbContext.Set<Payment>()
                                   .Where(predicate)
                                   .Include(x => x.DebtDetailUser)
                                   .ToListAsync();
        }
    }
}
