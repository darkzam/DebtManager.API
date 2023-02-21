using DebtManager.API.Filters;
using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;
using Microsoft.AspNetCore.Mvc;

public class DeleteDebtDetailUser : BaseEndpoint<DebtDetailUser>
{
    public DeleteDebtDetailUser(Uri baseRoute, WebApplication webApplication) : base(baseRoute, webApplication)
    { }

    public override void Initialize()
    {
        WebApplication.MapDelete($"{BasePath.OriginalString}Charges/{{chargeId}}", ProcessRequest)
                      .WithTags("Charges")
                      .AddEndpointFilter<AuthorizationFilter>()
                      .AddEndpointFilter<DebtValidatorFilter>();
    }

    private async Task<IResult> ProcessRequest([FromRoute] string debtCode,
                                               [FromRoute] Guid chargeId,
                                               IUnitOfWork unitOfWork,
                                               HttpContext context)
    {
        if (chargeId == Guid.Empty)
        {
            return Results.BadRequest($"{nameof(chargeId)} is not valid.");
        }

        var charge = await unitOfWork.DebtDetailUserRepository.Find(chargeId);

        if (charge is null)
        {
            return Results.BadRequest($"{nameof(chargeId)} is not in the system.");
        }

        unitOfWork.DebtDetailUserRepository.Remove(charge);

        await unitOfWork.CompleteAsync();

        return Results.NoContent();
    }
}
