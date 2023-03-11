using DebtManager.API.Models;
using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;

namespace DebtManager.API.Filters
{
    public class InvoiceResultFilter : IEndpointFilter
    {
        private readonly IUnitOfWork _unitOfWork;

        public InvoiceResultFilter(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var result = await next(context);

            var property = result.GetType().GetProperty("Value");

            if (property != null)
            {
                return result;
            }

            var debt = context.HttpContext.Items["debt"] as Debt;

            var prices = await _unitOfWork.PriceRepository.SearchBy(x => x.Business.Id == debt.Business.Id);

            var latestPrices = prices.GroupBy(x => x.Product)
                                     .Select(x => x.OrderByDescending(y => y.Date).First());

            var currentDetails = await _unitOfWork.DebtDetailRepository.SearchBy(x => x.Debt.Id == debt.Id);

            var currentDetailsIds = currentDetails.Select(x => x.Id);

            var currentCharges = await _unitOfWork.DebtDetailUserRepository.SearchBy(x => currentDetailsIds.Contains(x.DebtDetail.Id));

            var groupedCharges = currentCharges.GroupBy(x => x.DebtDetail).Select(x => new { DebtDetail = x.Key, Total = x.Sum(y => y.Porcentage), Users = x.Select(z => z.User) });

            var leftJoin = currentDetails.GroupJoin(groupedCharges, x => x.Id, y => y.DebtDetail.Id, (x, y) => new DebtDetailChargesEntry { DebtDetail = x, Total = y.FirstOrDefault()?.Total ?? 0, Users = y.SelectMany(z => z.Users).ToList() }).ToList();

            decimal ipoconsumo = (decimal)(debt.IpoconsumoTax ? 8.0 / 100 : 0);

            var groupByProduct = leftJoin.GroupBy(x => x.DebtDetail.Product).Join(latestPrices, x => x.Key.Id, y => y.Product.Id, (x, p) => new
            {
                ProductName = x.Key.Name,
                Amount = x.Count(),
                Price = p.Value,
                Subtotal = (x.Count() * p.Value),
                Total = (x.Count() * p.Value) * (1 + (debt.ServiceRate / 100) + ipoconsumo),
                ItemDetails = x.Select(x => new
                {
                    Coverage = x.Total,
                    Charges = currentCharges.Where(y => y.DebtDetail.Id == x.DebtDetail.Id).Select(z => new
                    {
                        Id = z.Id.ToString(),
                        Username = z.User.Username,
                        Value = z.Porcentage,
                        Subtotal = p.Value * (z.Porcentage / 100),
                        Total = (p.Value * (z.Porcentage / 100)) * (1 + (debt.ServiceRate / 100) + ipoconsumo)
                    })
                })
            });

            return Results.Ok(groupByProduct);
        }
    }
}
