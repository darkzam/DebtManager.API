using DebtManager.API.Filters;
using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;
using Microsoft.AspNetCore.Mvc;

public class PatchDebtDetailCollection : BaseEndpoint<DebtDetail>
{
    public PatchDebtDetailCollection(Uri baseRoute, WebApplication webApplication) : base(baseRoute, webApplication)
    { }

    public override void Initialize()
    {
        WebApplication.MapPost($"{Route.OriginalString}s", ProcessRequest)
                      .AddEndpointFilter<DebtValidatorFilter>()
                      .AddEndpointFilter<DebtDetailGroupValidatorFilter>();
    }

    private async Task<IResult> ProcessRequest([FromRoute] string debtCode,
                                               [FromBody] IEnumerable<DebtDetailGroupDto> debtDetailGroups,
                                               IUnitOfWork unitOfWork,
                                               HttpContext context)
    {
        var debt = context.Items["debt"] as Debt;
        var curatedProducts = context.Items["productPrices"] as IEnumerable<DebtDetailGroup>;

        foreach (var group in curatedProducts)
        {
            //PatchGroup(group, unitOfWork);

            //check if referred group product exists for current debt
            // if yes matches amount of debtDetail records either by removing or adding.
            
            //decide to update price or not at this point
        }

        return null;
    }
}

