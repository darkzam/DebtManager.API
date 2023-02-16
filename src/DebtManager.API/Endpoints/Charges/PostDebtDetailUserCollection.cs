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

        }



        //Add filter for result 

        return Results.Ok();
    }

    //private async Task<ProcessDetailGroupResult> ProcessProductCharges(Debt debt,
    //                                                                   ProductCharges productCharges,
    //                                                                   IUnitOfWork unitOfWork)
    //{
    //    var product = (await unitOfWork.ProductRepository.SearchBy(x => x.Name == productCharges.ProductName))
    //                                                     .FirstOrDefault();

    //    if (product is null)
    //    {
    //        return new ProcessDetailGroupResult()
    //        {
    //            Success = false,
    //            Message = "Product not found",
    //        };
    //    }

    //    //To-DO: Make sure to grab unassigned Details only. Disjoined from the Charges (DebtDetailUser) table.
    //    var currentDetails = await unitOfWork.DebtDetailRepository.SearchBy(x => x.Debt.Id == debt.Id
    //                                                                          && x.Product.Id == product.Id);

    //    var difference = debtDetailGroup.Amount > currentDetails.Count() ?
    //                     debtDetailGroup.Amount - currentDetails.Count() :
    //                     currentDetails.Count() - debtDetailGroup.Amount;

    //    if (difference == 0)
    //    {
    //        return new ProcessDetailGroupResult()
    //        {
    //            Success = true,
    //            DebtDetails = new List<DebtDetail>(),
    //            Operation = EntityOperation.None
    //        };
    //    }

    //    if (debtDetailGroup.Amount > currentDetails.Count())
    //    {
    //        var debtDetails = new List<DebtDetail>();
    //        for (int i = 0; i < difference; i++)
    //        {
    //            debtDetails.Add(new DebtDetail()
    //            {
    //                Debt = debt,
    //                Product = product,
    //                CreatedDate = DateTime.UtcNow,
    //                UpdatedDate = DateTime.UtcNow
    //            });
    //        }

    //        return new ProcessDetailGroupResult()
    //        {
    //            Success = true,
    //            DebtDetails = debtDetails,
    //            Operation = EntityOperation.Add
    //        };
    //    }

    //    return new ProcessDetailGroupResult()
    //    {
    //        Success = true,
    //        DebtDetails = currentDetails.Take(difference),
    //        Operation = EntityOperation.Remove
    //    };
    //}
}
