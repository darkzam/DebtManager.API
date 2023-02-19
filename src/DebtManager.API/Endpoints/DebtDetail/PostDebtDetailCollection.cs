using DebtManager.API.Filters;
using DebtManager.API.Models;
using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;
using Microsoft.AspNetCore.Mvc;

public class PostDebtDetailCollection : BaseEndpoint<DebtDetail>
{
    public PostDebtDetailCollection(Uri baseRoute, WebApplication webApplication) : base(baseRoute, webApplication)
    { }

    public override void Initialize()
    {
        WebApplication.MapPost($"{Route.OriginalString}s", ProcessRequest)
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
        foreach (var debtDetailGroup in curatedProducts)
        {
            var result = await ProcessDetailGroup(debt,
                                                  debtDetailGroup,
                                                  unitOfWork,
                                                  overridePrice);

            results.Add(result);
        }

        var addDebtDetails = results.Where(x => x.DebtDetails != null
                                          && x.DebtDetails.Any()
                                          && x.Operation == EntityOperation.Add)
                                 .SelectMany(x => x.DebtDetails);

        unitOfWork.DebtDetailRepository.CreateCollection(addDebtDetails);

        await unitOfWork.CompleteAsync();

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
                                                           Total = x.Count() * y.Value,
                                                           Price = y.Value
                                                       });

        var addedDetails = results.SelectMany(x => x.DebtDetails?.Select(y => y));

        var addedGroupDtos = addedDetails.GroupBy(x => x.Product)
                                         .Join(latestPrices,
                                                x => x.Key.Id,
                                                y => y.Product.Id,
                                                (x, y) => new DebtDetailGroupDto()
                                                {
                                                    ProductName = x.Key.Name,
                                                    Amount = x.Count(),
                                                    Total = x.Count() * y.Value,
                                                    Price = y.Value
                                                });

        return Results.Ok(new
        {
            Current = currentGroupDtos,
            Added = addedGroupDtos
        });
    }

    private async Task<ProcessDetailGroupResult> ProcessDetailGroup(Debt debt,
                                                                    DebtDetailGroup detailGroup,
                                                                    IUnitOfWork unitOfWork,
                                                                    bool? overridePrice)
    {
        var products = await unitOfWork.ProductRepository.SearchBy(x => x.Name == detailGroup.ProductName);

        var product = products.FirstOrDefault() != null ? products.FirstOrDefault() :
                                                            new Product()
                                                            {
                                                                Name = detailGroup.ProductName,
                                                                CreatedDate = DateTime.UtcNow,
                                                                UpdatedDate = DateTime.UtcNow
                                                            };

        if (!overridePrice.HasValue || overridePrice.Value)
        {
            unitOfWork.PriceRepository.Create(new Price()
            {
                Business = debt.Business,
                Product = product,
                Date = DateTime.UtcNow,
                Value = detailGroup.Price
            });
        }

        var debtDetails = new List<DebtDetail>();
        for (var i = 0; i < detailGroup.Amount; i++)
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
        };
    }
}
