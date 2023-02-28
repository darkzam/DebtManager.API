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
                      .WithTags("Charges")
                      .AddEndpointFilter<AuthorizationFilter>()
                      .AddEndpointFilter<DebtValidatorFilter>()
                      .AddEndpointFilter<InvoiceResultFilter>();
        //Add filter for validation of charges
    }

    private async Task<IResult> ProcessRequest([FromRoute] string debtCode,
                                               [FromBody] IEnumerable<ProductCharges> productChargesCollection,
                                               IUnitOfWork unitOfWork,
                                               HttpContext context)
    {
        var debt = context.Items["debt"] as Debt;

        var results = new List<ProcessProductChargesResult>();
        foreach (var productCharges in productChargesCollection)
        {
            var result = await ProcessProductCharges(debt,
                                            productCharges,
                                            unitOfWork);

            results.Add(result);
        }

        var error = results.Where(x => !x.Success).FirstOrDefault();

        if (error != null)
        {
            return Results.BadRequest(error.Message);
        }

        var addCharges = results.Where(x => x.Charges != null
                                           && x.Charges.Any()
                                           && x.Operation == EntityOperation.Add)
                                   .SelectMany(x => x.Charges);

        unitOfWork.DebtDetailUserRepository.CreateCollection(addCharges);

        await unitOfWork.CompleteAsync();

        return Results.Ok();
    }

    private async Task<ProcessProductChargesResult> ProcessProductCharges(Debt debt,
                                                                       ProductCharges productCharges,
                                                                       IUnitOfWork unitOfWork)
    {
        var product = (await unitOfWork.ProductRepository.SearchBy(x => x.Name == productCharges.ProductName.Trim()
                                                                                                           .ToLower()
                                                                                                           .RemoveAccents()))
                                                         .FirstOrDefault();

        if (product is null)
        {
            return new ProcessProductChargesResult()
            {
                Success = false,
                Message = "Product not found",
            };
        }

        var currentDetails = await unitOfWork.DebtDetailRepository.SearchBy(x => x.Debt.Id == debt.Id
                                                                              && x.Product.Id == product.Id);

        var currentDetailsIds = currentDetails.Select(x => x.Id);

        var currentCharges = await unitOfWork.DebtDetailUserRepository.SearchBy(x => currentDetailsIds.Contains(x.DebtDetail.Id));

        var groupedCharges = currentCharges.GroupBy(x => x.DebtDetail).Select(x => new { DebtDetail = x.Key, Total = x.Sum(y => y.Porcentage), Users = x.Select(z => z.User) });

        var leftJoin = currentDetails.GroupJoin(groupedCharges, x => x.Id, y => y.DebtDetail.Id, (x, y) => new DebtDetailChargesEntry { DebtDetail = x, Total = y.FirstOrDefault()?.Total ?? 0, Users = y.SelectMany(z => z.Users).ToList() }).ToList();

        var newCharges = new List<DebtDetailUser>();

        foreach (var charge in productCharges.Charges)
        {
            //To-Do: Optimize DB roundtrips.
            var user = (await unitOfWork.UserRepository.SearchBy(x => x.Username == charge.Username)).FirstOrDefault();

            if (user is null)
            {
                return new ProcessProductChargesResult()
                {
                    Success = false,
                    Message = $"Username not found: {charge.Username}"
                };
            }

            for (int i = 0; i < leftJoin.Count(); i++)
            {
                if (leftJoin[i].Total + charge.Value > 100
                    || leftJoin[i].Users.Contains(user))
                {
                    if (i == leftJoin.Count() - 1)
                    {
                        return new ProcessProductChargesResult()
                        {
                            Success = false,
                            Message = $"Charge could not be assigned: {{ Username = {charge.Username}, Value = {charge.Value} }} ",
                        };
                    }

                    continue;
                }

                leftJoin[i].Total += charge.Value;
                leftJoin[i].Users.Add(user);

                newCharges.Add(new DebtDetailUser()
                {
                    DebtDetail = leftJoin[i].DebtDetail,
                    User = user,
                    Porcentage = charge.Value
                });

                break;
            }
        }

        return new ProcessProductChargesResult()
        {
            Success = true,
            Charges = newCharges,
            Operation = EntityOperation.Add
        };
    }
}
