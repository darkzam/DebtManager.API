using DebtManager.API.Filters;
using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;

namespace DebtManager.API.Endpoints.Reports
{
    public class GetReportDebts : BaseEndpoint<DebtDetailUser>
    {
        public GetReportDebts(Uri baseRoute, WebApplication webApplication) : base(baseRoute, webApplication)
        { }

        public override void Initialize()
        {
            WebApplication.MapGet($"{BasePath.OriginalString}Debts", ProcessRequest)
                          .WithTags("Reports")
                          .AddEndpointFilter<AuthorizationFilter>();
        }

        private async Task<IResult> ProcessRequest(IUnitOfWork unitOfWork)
        {
            var debts = await unitOfWork.DebtRepository.SearchBy(x => x.CreatedDate.Date.AddDays(3) < DateTimeOffset.Now.Date);

            var debtIds = debts.Select(y => y.Id);

            var prices = await unitOfWork.PriceRepository.GetAll();

            var latestPrices = prices.GroupBy(x => new { x.Business, x.Product })
                                     .Select(x => x.OrderByDescending(y => y.Date).First());

            var currentDetails = await unitOfWork.DebtDetailRepository.SearchBy(x => debtIds.Contains(x.Debt.Id));

            var currentDetailsIds = currentDetails.Select(x => x.Id);

            var currentCharges = await unitOfWork.DebtDetailUserRepository.SearchBy(x => currentDetailsIds.Contains(x.DebtDetail.Id));

            var currentPayments = await unitOfWork.PaymentRepository.SearchBy(x => currentCharges.Select(x => x.Id).Contains(x.DebtDetailUser.Id));

            var unpaidCurrentCharges = currentCharges.Except(currentPayments.Select(x => x.DebtDetailUser));

            var currentChargesWithPricing = unpaidCurrentCharges.Join(latestPrices,
                                                                w => new { w.DebtDetail.Debt.Business, w.DebtDetail.Product },
                                                                p => new { p.Business, p.Product },
                                                                (w, p) => new
                                                                {
                                                                    w.DebtDetail.Debt,
                                                                    w.User,
                                                                    ProductName = w.DebtDetail.Product.Name,
                                                                    Price = p.Value,
                                                                    Value = w.Porcentage,
                                                                    Subtotal = p.Value * (w.Porcentage / 100),
                                                                    Total = (p.Value * (w.Porcentage / 100)) * (1 + (w.DebtDetail.Debt.ServiceRate / 100) + (decimal)(w.DebtDetail.Debt.IpoconsumoTax ? 8.0 / 100 : 0))
                                                                });

            var groupByUser = currentChargesWithPricing.GroupBy(x => new { x.Debt, x.User })
                                                        .Select(x => new
                                                        {
                                                            x.Key.Debt,
                                                            x.Key.User.Username,
                                                            TotalPayment = x.Sum(y => y.Total),
                                                            Charges = x.Select(y => new
                                                            {
                                                                y.ProductName,
                                                                y.Price,
                                                                y.Value,
                                                                y.Subtotal,
                                                                y.Total
                                                            })
                                                        });

            var groupByDebts = groupByUser.GroupBy(x => x.Debt)
                                           .Select(x => new
                                           {
                                               DebtTitle = x.Key.Title,
                                               Date = x.Key.CreatedDate.ToString("dd/MM/yyyy"),
                                               GrandTotal = x.Sum(y => y.TotalPayment),
                                               Debtors = x.Select(y => new
                                               {
                                                   y.Username,
                                                   y.TotalPayment,
                                                   y.Charges
                                               })
                                           });

            return Results.Ok(groupByDebts);
        }
    }
}
