using DebtManager.API.Filters;
using DebtManager.API.Models;
using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;
using Microsoft.AspNetCore.Mvc;

public class PatchDebtDetailCollection : BaseEndpoint<DebtDetail>
{
    public PatchDebtDetailCollection(Uri baseRoute, WebApplication webApplication) : base(baseRoute, webApplication)
    { }

    public override void Initialize()
    {
        WebApplication.MapPatch($"{Route.OriginalString}s", ProcessRequest)
                      .WithTags("DebtDetails")
                      .AddEndpointFilter<AuthorizationFilter>()
                      .AddEndpointFilter<DebtValidatorFilter>()
                      .AddEndpointFilter<DebtDetailGroupValidatorFilter>();
    }

    private async Task<IResult> ProcessRequest([FromRoute] string debtCode,
                                               [FromBody] IEnumerable<DebtDetailGroupDto> debtDetailGroups,
                                               [FromQuery] bool? overridePrice,
                                               IUnitOfWork unitOfWork,
                                               HttpContext context)
    {
        var debt = context.Items["debt"] as Debt;
        var curatedProducts = context.Items["productPrices"] as IEnumerable<DebtDetailGroup>;

        var results = new List<ProcessDetailGroupResult>();
        foreach (var group in curatedProducts)
        {
            var result = await ProcessDetailGroup(debt,
                                                  group,
                                                  unitOfWork,
                                                  overridePrice);

            results.Add(result);
        }

        var addDebtDetails = results.Where(x => x.DebtDetails != null
                                            && x.DebtDetails.Any()
                                            && x.Operation == EntityOperation.Add)
                                    .SelectMany(x => x.DebtDetails);

        unitOfWork.DebtDetailRepository.CreateCollection(addDebtDetails);

        var removeDebtDetails = results.Where(x => x.DebtDetails != null
                                            && x.DebtDetails.Any()
                                            && x.Operation == EntityOperation.Remove)
                                       .SelectMany(x => x.DebtDetails);

        unitOfWork.DebtDetailRepository.RemoveCollection(removeDebtDetails);

        await unitOfWork.CompleteAsync();

        var currentDebtDetails = await unitOfWork.DebtDetailRepository.SearchBy(x => x.Debt.Id == debt.Id);

        var prices = await unitOfWork.PriceRepository.SearchBy(x => x.Business.Id == debt.Business.Id);

        var latestPrices = prices.GroupBy(x => x.Product)
                                 .Select(x => x.OrderByDescending(y => y.Date).First());

        var currentGroupDtos = currentDebtDetails.GroupBy(x => x.Product)
                                                 .Join(latestPrices,
                                                       x => x.Key?.Id,
                                                       y => y.Product?.Id,
                                                       (x, y) => new DebtDetailGroupDto()
                                                       {
                                                           ProductName = x.Key?.Name,
                                                           Amount = x.Count(),
                                                           Total = x.Count() * y.Value,
                                                           Price = y.Value
                                                       });

        var addedGroupDtos = addDebtDetails.GroupBy(x => x.Product)
                                           .Join(latestPrices,
                                                x => x.Key?.Id,
                                                y => y.Product?.Id,
                                                (x, y) => new DebtDetailGroupDto()
                                                {
                                                    ProductName = x.Key?.Name,
                                                    Amount = x.Count(),
                                                    Total = x.Count() * y.Value,
                                                    Price = y.Value
                                                });

        var removedGroupDtos = removeDebtDetails.GroupBy(x => x.Product)
                                                .Join(latestPrices,
                                                    x => x.Key?.Id,
                                                    y => y.Product?.Id,
                                                    (x, y) => new DebtDetailGroupDto()
                                                    {
                                                        ProductName = x.Key?.Name,
                                                        Amount = x.Count(),
                                                        Total = x.Count() * y.Value,
                                                        Price = y.Value
                                                    });

        return Results.Ok(new
        {
            Current = currentGroupDtos,
            Added = addedGroupDtos,
            Removed = removedGroupDtos
        });
    }

    private async Task<ProcessDetailGroupResult> ProcessDetailGroup(Debt debt,
                                                                    DebtDetailGroup debtDetailGroup,
                                                                    IUnitOfWork unitOfWork,
                                                                    bool? overridePrice)
    {
        var product = (await unitOfWork.ProductRepository.SearchBy(x => x.Name == debtDetailGroup.ProductName))
                                                         .FirstOrDefault();

        if (product is null)
        {
            return new ProcessDetailGroupResult()
            {
                Success = false,
                Message = "Product not found",
            };
        }

        if ((!overridePrice.HasValue || overridePrice.Value)
            && debtDetailGroup.Price > 0)
        {
            unitOfWork.PriceRepository.Create(new Price()
            {
                Business = debt.Business,
                Product = product,
                Date = DateTime.UtcNow,
                Value = debtDetailGroup.Price
            });
        }

        //To-DO: Make sure to grab unassigned Details only. Disjoined from the Charges (DebtDetailUser) table.
        var currentDetails = await unitOfWork.DebtDetailRepository.SearchBy(x => x.Debt.Id == debt.Id
                                                                              && x.Product.Id == product.Id);

        var difference = debtDetailGroup.Amount > currentDetails.Count() ?
                         debtDetailGroup.Amount - currentDetails.Count() :
                         currentDetails.Count() - debtDetailGroup.Amount;

        if (difference == 0)
        {
            return new ProcessDetailGroupResult()
            {
                Success = true,
                DebtDetails = new List<DebtDetail>(),
                Operation = EntityOperation.None
            };
        }

        if (debtDetailGroup.Amount > currentDetails.Count())
        {
            var debtDetails = new List<DebtDetail>();
            for (int i = 0; i < difference; i++)
            {
                debtDetails.Add(new DebtDetail()
                {
                    Debt = debt,
                    Product = product,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                });
            }

            return new ProcessDetailGroupResult()
            {
                Success = true,
                DebtDetails = debtDetails,
                Operation = EntityOperation.Add
            };
        }

        return new ProcessDetailGroupResult()
        {
            Success = true,
            DebtDetails = currentDetails.Take(difference),
            Operation = EntityOperation.Remove
        };
    }
}

