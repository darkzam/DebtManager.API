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
        WebApplication.MapPost($"{Route.OriginalString}s", ProcessRequest);
    }

    private async Task<IResult> ProcessRequest([FromRoute] string debtCode,
                                               [FromBody] IEnumerable<DebtDetailGroupDto> debtDetailGroups,
                                               IUnitOfWork unitOfWork)
    {
        if (string.IsNullOrWhiteSpace(debtCode))
        {
            return Results.BadRequest($"Provided {nameof(debtCode)} is invalid.");
        }

        var debts = await unitOfWork.DebtRepository.SearchBy(x => x.Code == debtCode
                                                                            .Trim()
                                                                            .ToLower()
                                                                            .RemoveAccents());

        if (debts.FirstOrDefault() is null)
        {
            return Results.NotFound($"{nameof(debtCode)} provided does not exist in the system.");
        }

        if (debtDetailGroups is null || !debtDetailGroups.Any())
        {
            return Results.BadRequest(nameof(debtDetailGroups));
        }

        var invalidModels = debtDetailGroups.Select(x => x.ValidateModel())
                                            .Where(x => !x.Success);

        if (invalidModels.Any())
        {
            return Results.BadRequest(invalidModels.Select(x => x.Message));
        }

        debtDetailGroups = debtDetailGroups.Select(x => new DebtDetailGroupDto()
        {
            ProductName = x.ProductName.Trim()
                                       .ToLower()
                                       .RemoveAccents(),
            Amount = x.Amount,
            Total = x.Total
        });

        var productPrices = debtDetailGroups.Select(x =>
                                                new DebtDetailGroup()
                                                {
                                                    ProductName = x.ProductName,
                                                    Price = x.Total / x.Amount,
                                                    Amount = x.Amount,
                                                })
                                             .GroupBy(x => new
                                             {
                                                 x.ProductName,
                                                 x.Price
                                             });

        var multiplePricedProduct = productPrices.GroupBy(x => x.Key.ProductName)
                                                 .Where(group => group.Count() > 1);

        if (multiplePricedProduct.Any())
        {
            return Results.BadRequest($"Multiple different prices were submitted for '{multiplePricedProduct.First().Key}'");
        }

        var curatedProducts = productPrices.Select(group => new DebtDetailGroup()
        {
            ProductName = group.Key.ProductName,
            Amount = group.Sum(x => x.Amount),
            Price = group.Key.Price
        });

        var results = new List<ProcessDetailGroupResult>();
        foreach (var debtDetailGroup in curatedProducts)
        {
            var result = await ProcessDetailGroup(debts.FirstOrDefault(),
                                                  debtDetailGroup,
                                                  unitOfWork);

            results.Add(result);
        }

        var debtDetails = results.Where(x => x.DebtDetails != null && x.DebtDetails.Any()).SelectMany(x => x.DebtDetails);

        unitOfWork.DebtDetailRepository.CreateCollection(debtDetails);

        await unitOfWork.CompleteAsync();

        var currentDebtDetails = await unitOfWork.DebtDetailRepository.SearchBy(x => x.Debt.Id == debts.FirstOrDefault().Id);

        var currentGroupDtos = currentDebtDetails.Select(x => new DebtDetailDto(x))
                                                 .GroupBy(x => x.ProductName)
                                                 .Select(group => new DebtDetailGroupDto()
                                                 {
                                                     ProductName = group.Key,
                                                     Amount = group.Count()
                                                 });

        var addedDtos = results.SelectMany(x => x.DebtDetails?.Select(y => new DebtDetailDto(y)));

        var addedGroupDtos = addedDtos.GroupBy(x => x.ProductName).Select(group => new DebtDetailGroupDto()
        {
            ProductName = group.Key,
            Amount = group.Count()
        });

        return Results.Ok(new
        {
            Current = currentGroupDtos,
            Added = addedGroupDtos
        });
    }

    private async Task<ProcessDetailGroupResult> ProcessDetailGroup(Debt debt,
                                                                    DebtDetailGroup detailGroup,
                                                                    IUnitOfWork unitOfWork)
    {
        var products = await unitOfWork.ProductRepository.SearchBy(x => x.Name == detailGroup.ProductName);

        var product = products.FirstOrDefault() != null ? products.FirstOrDefault() :
                                                            new Product()
                                                            {
                                                                Name = detailGroup.ProductName,
                                                                CreatedDate = DateTime.UtcNow,
                                                                UpdatedDate = DateTime.UtcNow
                                                            };

        var prices = new List<Price>();
        if (products.FirstOrDefault() != null)
        {
            prices = (await unitOfWork.PriceRepository.SearchBy(x => x.Product.Id == product.Id)).ToList();
        }

        if (prices.FirstOrDefault() is null)
        {
            unitOfWork.PriceRepository.Create(new Price()
            {
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
