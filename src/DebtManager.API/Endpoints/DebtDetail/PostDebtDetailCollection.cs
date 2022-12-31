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

        var taskResults = debtDetailGroups.Select(x => ProcessDetailGroup(debts.FirstOrDefault(),
                                                                          x,
                                                                          unitOfWork));

        var results = await Task.WhenAll(taskResults);

        var debtDetails = results.Where(x => x.DebtDetails != null && x.DebtDetails.Any()).SelectMany(x => x.DebtDetails);

        unitOfWork.DebtDetailRepository.CreateCollection(debtDetails);

        await unitOfWork.CompleteAsync();

        //To-Do map DebtDetail entities to Dtos before returning
        return Results.Ok(results);
    }

    private async Task<ProcessDetailGroupResult> ProcessDetailGroup(Debt debt,
                                                                    DebtDetailGroupDto detailGroup,
                                                                    IUnitOfWork unitOfWork)
    {
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
