using DebtManager.API.Filters;
using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;
using Microsoft.AspNetCore.Mvc;

public class DeletePayment : BaseEndpoint<Payment>
{
    public DeletePayment(Uri baseRoute, WebApplication webApplication) : base(baseRoute, webApplication)
    { }

    public override void Initialize()
    {
        WebApplication.MapDelete($"{BasePath.OriginalString}Payments/{{paymentId}}", ProcessRequest)
                      .WithTags("Payments")
                      .AddEndpointFilter<AuthorizationFilter>()
                      .AddEndpointFilter<DebtValidatorFilter>();
    }

    private async Task<IResult> ProcessRequest([FromRoute] string debtCode,
                                               [FromRoute] Guid paymentId,
                                               IUnitOfWork unitOfWork,
                                               HttpContext context)
    {
        if (paymentId == Guid.Empty)
        {
            return Results.BadRequest($"{nameof(paymentId)} is not valid.");
        }

        var payment = await unitOfWork.DebtDetailUserRepository.Find(paymentId);

        if (payment is null)
        {
            return Results.BadRequest($"{nameof(paymentId)} is not in the system.");
        }

        unitOfWork.DebtDetailUserRepository.Remove(payment);

        await unitOfWork.CompleteAsync();

        return Results.NoContent();
    }
}
