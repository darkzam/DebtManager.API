using DebtManager.API.Filters;
using DebtManager.API.Models;
using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;
using Microsoft.AspNetCore.Mvc;

public class PatchPaymentCollection : BaseEndpoint<Payment>
{
    public PatchPaymentCollection(Uri baseRoute, WebApplication webApplication) : base(baseRoute, webApplication)
    { }

    public override void Initialize()
    {
        WebApplication.MapPatch($"{BasePath.OriginalString}Payments", ProcessRequest)
                      .WithTags("Payments")
                      .AddEndpointFilter<AuthorizationFilter>()
                      .AddEndpointFilter<DebtValidatorFilter>();
    }

    private async Task<IResult> ProcessRequest([FromRoute] string debtCode,
                                               [FromBody] IEnumerable<PatchPaymentDto> payments,
                                               IUnitOfWork unitOfWork,
                                               HttpContext context)
    {
        var debt = context.Items["debt"] as Debt;

        var results = new List<ProcessPaymentResult>();
        foreach (var payment in payments)
        {
            var result = await ProcessPayment(payment,
                                              unitOfWork);

            results.Add(result);
        }

        var error = results.Where(x => !x.Success).FirstOrDefault();

        if (error != null)
        {
            return Results.BadRequest(error.Message);
        }

        var updatedPayments = results.Where(x => x.Payment != null
                                           && x.Operation == EntityOperation.Update)
                                   .Select(x => x.Payment);

        unitOfWork.PaymentRepository.UpdateCollection(updatedPayments);

        await unitOfWork.CompleteAsync();

        return Results.Ok(updatedPayments);
    }

    private async Task<ProcessPaymentResult> ProcessPayment(PatchPaymentDto payment,
                                                            IUnitOfWork unitOfWork)
    {
        var currentPayment = (await unitOfWork.PaymentRepository.SearchBy(x => x.Id == payment.Id)).FirstOrDefault();

        if (currentPayment is null)
        {
            return new ProcessPaymentResult()
            {
                Success = false,
                Message = "PaymentId not found.",
            };
        }

        var status = PaymentStatus.None;

        if (!Enum.TryParse(payment.Status, out status))
        {
            return new ProcessPaymentResult()
            {
                Success = false,
                Message = "Status is not valid.",
            };
        }

        currentPayment.Status = status;
        currentPayment.UpdatedDate = DateTime.Now;

        return new ProcessPaymentResult()
        {
            Success = true,
            Payment = currentPayment,
            Operation = EntityOperation.Update
        };
    }
}
