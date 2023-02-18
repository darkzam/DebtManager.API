using DebtManager.API.Filters;
using DebtManager.API.Models;
using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;
using Microsoft.AspNetCore.Mvc;

public class PostDebtDetailUserCollection : BaseEndpoint<DebtDetailUser>
{
    public PostDebtDetailUserCollection(Uri baseRoute, WebApplication webApplication) : base(baseRoute, webApplication)
    { }

    public override void Initialize()
    {
        WebApplication.MapPost($"{BasePath.OriginalString}Charges", ProcessRequest)
                      .AddEndpointFilter<DebtValidatorFilter>();
        //Add filter for validation of charges
    }

    private async Task<IResult> ProcessRequest([FromRoute] string debtCode,
                                               [FromBody] IEnumerable<ProductCharges> productChargesCollection,
                                               IUnitOfWork unitOfWork,
                                               HttpContext context)
    {
        var debt = context.Items["debt"] as Debt;

        foreach (var productCharges in productChargesCollection)
        {
            _ = await ProcessProductCharges(debt,
                                            productCharges,
                                            unitOfWork);
        }



        //Add filter for result 

        return Results.Ok();
    }

    private async Task<ProcessDetailGroupResult> ProcessProductCharges(Debt debt,
                                                                       ProductCharges productCharges,
                                                                       IUnitOfWork unitOfWork)
    {
        var product = (await unitOfWork.ProductRepository.SearchBy(x => x.Name == productCharges.ProductName.Trim()
                                                                                                           .ToLower()
                                                                                                           .RemoveAccents()))
                                                         .FirstOrDefault();

        if (product is null)
        {
            return new ProcessDetailGroupResult()
            {
                Success = false,
                Message = "Product not found",
            };
        }

        //Compute current item charges status entry by entry
        //and see how much of their 100% is assigned.

        //ideally all entries(DebtDetails)'s charges should total up to 100%.
        //generate current Debt's DebtDetail Charges enumerable.
        var currentDetails = await unitOfWork.DebtDetailRepository.SearchBy(x => x.Debt.Id == debt.Id
                                                                              && x.Product.Id == product.Id);

        var currentDetailsIds = currentDetails.Select(x => x.Id);

        var currentCharges = await unitOfWork.DebtDetailUserRepository.SearchBy(x => currentDetailsIds.Contains(x.DebtDetail.Id));

        var groupedCharges = currentCharges.GroupBy(x => x.DebtDetail).Select(x => new { DebtDetail = x.Key, Total = x.Sum(y => y.Porcentage) });

        var join = currentDetails.GroupJoin(groupedCharges, x => x.Id, y => y.DebtDetail.Id, (x, y) => new { DebtDetail = x.Id, Total = y.Any() ? y.First().Total : 0 });

        return null;
        
        //var difference = debtDetailGroup.Amount > currentDetails.Count() ?
        //                 debtDetailGroup.Amount - currentDetails.Count() :
        //                 currentDetails.Count() - debtDetailGroup.Amount;

        //if (difference == 0)
        //{
        //    return new ProcessDetailGroupResult()
        //    {
        //        Success = true,
        //        DebtDetails = new List<DebtDetail>(),
        //        Operation = EntityOperation.None
        //    };
        //}

        //if (debtDetailGroup.Amount > currentDetails.Count())
        //{
        //    var debtDetails = new List<DebtDetail>();
        //    for (int i = 0; i < difference; i++)
        //    {
        //        debtDetails.Add(new DebtDetail()
        //        {
        //            Debt = debt,
        //            Product = product,
        //            CreatedDate = DateTime.UtcNow,
        //            UpdatedDate = DateTime.UtcNow
        //        });
        //    }

        //    return new ProcessDetailGroupResult()
        //    {
        //        Success = true,
        //        DebtDetails = debtDetails,
        //        Operation = EntityOperation.Add
        //    };
        //}

        //return new ProcessDetailGroupResult()
        //{
        //    Success = true,
        //    DebtDetails = currentDetails.Take(difference),
        //    Operation = EntityOperation.Remove
        //};
    }
}
