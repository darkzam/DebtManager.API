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

        //To-Do find a way to prevent duplicity of products, add their amounts if they are the same one.

        var results = new List<ProcessDetailGroupResult>();
        foreach (var debtDetailGroup in debtDetailGroups)
        {
            var result = await ProcessDetailGroup(debts.FirstOrDefault(),
                                                  debtDetailGroup,
                                                  unitOfWork);

            results.Add(result);
        }

        var debtDetails = results.Where(x => x.DebtDetails != null && x.DebtDetails.Any()).SelectMany(x => x.DebtDetails);

        unitOfWork.DebtDetailRepository.CreateCollection(debtDetails);

        await unitOfWork.CompleteAsync();

        return Results.Ok(results.Select(x => new
        {
            Success = x.Success,
            Message = x.Message,
            DebtDetails = x.DebtDetails?.Select(y => new DebtDetailDto(y))
        }));
    }

    private async Task<ProcessDetailGroupResult> ProcessDetailGroup(Debt debt,
                                                                    DebtDetailGroupDto detailGroup,
                                                                    IUnitOfWork unitOfWork)
    {
        if (detailGroup.Amount == 0)
        {
            return new ProcessDetailGroupResult
            {
                Success = false,
                Message = $"{nameof(detailGroup.Amount)} is invalid."
            };
        }

        if (detailGroup.Total < 1)
        {
            return new ProcessDetailGroupResult
            {
                Success = false,
                Message = $"{nameof(detailGroup.Total)} is invalid."
            };
        }

        if (string.IsNullOrWhiteSpace(detailGroup.ProductName))
        {
            return new ProcessDetailGroupResult
            {
                Success = false,
                Message = $"{nameof(detailGroup.ProductName)} is invalid."
            };
        }

        var products = await unitOfWork.ProductRepository.SearchBy(x => x.Name == detailGroup.ProductName
                                                                                            .Trim()
                                                                                            .ToLower()
                                                                                            .RemoveAccents());

        var product = products.FirstOrDefault() != null ? products.FirstOrDefault() :
                                                            new Product()
                                                            {
                                                                Name = detailGroup.ProductName
                                                                                    .Trim()
                                                                                    .ToLower()
                                                                                    .RemoveAccents(),
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
            decimal value = detailGroup.Total / detailGroup.Amount;
            unitOfWork.PriceRepository.Create(new Price()
            {
                Product = product,
                Date = DateTime.UtcNow,
                Value = value
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
