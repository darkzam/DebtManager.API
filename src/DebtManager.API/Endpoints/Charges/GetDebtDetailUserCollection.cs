using DebtManager.API.Filters;
using DebtManager.API.Models;
using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;
using Microsoft.AspNetCore.Mvc;

public class GetDebtDetailUserCollection : BaseEndpoint<DebtDetailUser>
{
    public GetDebtDetailUserCollection(Uri baseRoute, WebApplication webApplication) : base(baseRoute, webApplication)
    { }

    public override void Initialize()
    {
        WebApplication.MapGet($"{BasePath.OriginalString}Charges", ProcessRequest)
                      .WithTags("Charges")
                      .AddEndpointFilter<AuthorizationFilter>()
                      .AddEndpointFilter<DebtValidatorFilter>();
        //Add filter for validation of charges
    }

    private async Task<IResult> ProcessRequest([FromRoute] string debtCode,
                                               [FromQuery] string? username,
                                               IUnitOfWork unitOfWork,
                                               HttpContext context)
    {
        var debt = context.Items["debt"] as Debt;

        if (!string.IsNullOrWhiteSpace(username))
        {
            username = username.Trim()
                               .ToLower()
                               .RemoveAccents()
                               .RemoveSpaces();

            var user = (await unitOfWork.UserRepository.SearchBy(x => x.Username == username)).FirstOrDefault();

            if (user is null)
            {
                return Results.NotFound("Username not found.");
            }

            var result = await GetChargesByUsername(user, unitOfWork);

            return result;
        }

        var currentDetails = await unitOfWork.DebtDetailRepository.SearchBy(x => x.Debt.Id == debt.Id);

        var currentDetailsIds = currentDetails.Select(x => x.Id);

        var currentCharges = await unitOfWork.DebtDetailUserRepository.SearchBy(x => currentDetailsIds.Contains(x.DebtDetail.Id));

        var groupedCharges = currentCharges.GroupBy(x => x.DebtDetail).Select(x => new { DebtDetail = x.Key, Total = x.Sum(y => y.Porcentage), Users = x.Select(z => z.User) });

        var leftJoin = currentDetails.GroupJoin(groupedCharges, x => x.Id, y => y.DebtDetail.Id, (x, y) => new DebtDetailChargesEntry { DebtDetail = x, Total = y.FirstOrDefault()?.Total ?? 0, Users = y.SelectMany(z => z.Users).ToList() }).ToList();

        var groupByProduct = leftJoin.GroupBy(x => x.DebtDetail.Product).Select(x => new
        {
            ProductName = x.Key.Name,
            Amount = x.Count(),
            ItemDetails = x.Select(x => new
            {
                Total = x.Total,
                Charges = currentCharges.Where(y => y.DebtDetail.Id == x.DebtDetail.Id).Select(z => new ChargePorcentage()
                {
                    Id = z.Id.ToString(),
                    Username = z.User.Username,
                    Value = z.Porcentage
                })
            })
        });

        return Results.Ok(groupByProduct);
    }

    private async Task<IResult> GetChargesByUsername(User user,
                                                     IUnitOfWork unitOfWork)
    {
        var currentCharges = await unitOfWork.DebtDetailUserRepository.SearchBy(x => x.User.Id == user.Id);

        var groupByProduct = currentCharges.GroupBy(x => x.DebtDetail.Product).Select(x => new
        {
            ProductName = x.Key.Name,
            Charges = x.Select(y => new ChargePorcentage()
            {
                Id = y.Id.ToString(),
                Username = y.User.Username,
                Value = y.Porcentage
            })
        });

        return Results.Ok(groupByProduct);
    }
}
