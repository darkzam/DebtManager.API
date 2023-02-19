using DebtManager.API.Filters;
using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;
using Microsoft.AspNetCore.Mvc;

public class GetDebtDetailCollection : BaseEndpoint<DebtDetail>
{
    public GetDebtDetailCollection(Uri baseRoute, WebApplication webApplication) : base(baseRoute, webApplication)
    { }
    public override void Initialize()
    {
        WebApplication.MapGet($"{Route.OriginalString}s", ProcessRequest)
                      .WithTags("DebtDetails")
                      .AddEndpointFilter<AuthorizationFilter>()
                      .AddEndpointFilter<DebtValidatorFilter>();
    }//

    private async Task<IResult> ProcessRequest([FromRoute] string debtCode,
                                               IUnitOfWork unitOfWork,
                                               HttpContext context)
    {
        var debt = context.Items["debt"] as Debt;

        var currentDebtDetails = await unitOfWork.DebtDetailRepository.SearchBy(x => x.Debt.Id == debt.Id);

        var prices = await unitOfWork.PriceRepository.SearchBy(x => x.Business.Id == debt.Business.Id);

        var latestPrices = prices.GroupBy(x => x.Product)
                                 .Select(x => x.OrderByDescending(y => y.Date).First());

        var currentGroupDtos = currentDebtDetails.GroupBy(x => x.Product)
                                                 .Join(latestPrices,
                                                       x => x.Key.Id,
                                                       y => y.Product.Id,
                                                       (x, y) => new DebtDetailGroupDto()
                                                       {
                                                           ProductName = x.Key.Name,
                                                           Amount = x.Count(),
                                                           Price = y.Value,
                                                           Total = x.Count() * y.Value
                                                       });

        return Results.Ok(currentGroupDtos);
    }
}
