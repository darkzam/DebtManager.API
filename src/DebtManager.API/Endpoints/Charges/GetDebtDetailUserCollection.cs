using DebtManager.API.Filters;
using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;
using Microsoft.AspNetCore.Mvc;

public class GetDebtDetailUserCollection : BaseEndpoint<DebtDetailUser>
{
    public GetDebtDetailUserCollection(Uri baseRoute, WebApplication webApplication) : base(baseRoute, webApplication)
    { }

    public override void Initialize()
    {
        WebApplication.MapGet($"{BasePath.OriginalString}Charges", ProcessRequest)
                      .WithTags("Charges")
                      .AddEndpointFilter<AuthorizationFilter>()
                      .AddEndpointFilter<DebtValidatorFilter>()
                      .AddEndpointFilter<InvoiceResultFilter>();
        //Add filter for validation of charges
    }

    private async Task<IResult> ProcessRequest([FromRoute] string debtCode,
                                               [FromQuery] string? username,
                                               IUnitOfWork unitOfWork,
                                               HttpContext context)
    {
        var debt = context.Items["debt"] as Debt;

        if (!string.IsNullOrWhiteSpace(username))
        {
            username = username.Trim()
                               .ToLower()
                               .RemoveSpaces();

            var user = (await unitOfWork.UserRepository.SearchBy(x => x.Username == username)).FirstOrDefault();

            if (user is null)
            {
                return Results.NotFound("Username not found.");
            }

            var result = await GetChargesByUsername(debt,
                                                    user,
                                                    unitOfWork);

            return result;
        }

        return Results.Ok();
    }

    private async Task<IResult> GetChargesByUsername(Debt debt,
                                                     User user,
                                                     IUnitOfWork unitOfWork)
    {
        var prices = await unitOfWork.PriceRepository.SearchBy(x => x.Business.Id == debt.Business.Id);

        var latestPrices = prices.GroupBy(x => x.Product)
                                 .Select(x => x.OrderByDescending(y => y.Date).First());

        var currentCharges = await unitOfWork.DebtDetailUserRepository.SearchBy(x => x.User.Id == user.Id);

        decimal ipoconsumo = (decimal)(debt.IpoconsumoTax ? 8.0 / 100 : 0);

        var groupByProduct = currentCharges.GroupBy(x => x.DebtDetail.Product)
                                           .Join(latestPrices,
                                                 x => x.Key.Id,
                                                 y => y.Product.Id,
                                                (x, p) => new
                                                {
                                                    ProductName = x.Key.Name,
                                                    Price = p.Value,
                                                    Charges = x.Select(y => new
                                                    {
                                                        Id = y.Id.ToString(),
                                                        Username = y.User.Username,
                                                        Value = y.Porcentage,
                                                        Subtotal = p.Value * (y.Porcentage / 100),
                                                        Total = (p.Value * (y.Porcentage / 100)) * (1 + (debt.ServiceRate / 100) + ipoconsumo)
                                                    })
                                                });

        return Results.Ok(groupByProduct);
    }
}
