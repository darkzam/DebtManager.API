using DebtManager.API.Filters;
using DebtManager.API.Models;
using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;
using Microsoft.AspNetCore.Mvc;

public class PostPaymentCollection : BaseEndpoint<Payment>
{
    public PostPaymentCollection(Uri baseRoute, WebApplication webApplication) : base(baseRoute, webApplication)
    { }

    public override void Initialize()
    {
        WebApplication.MapPost($"{BasePath.OriginalString}Payments", ProcessRequest)
                      .WithTags("Payments")
                      .AddEndpointFilter<AuthorizationFilter>()
                      .AddEndpointFilter<DebtValidatorFilter>();
    }

    private async Task<IResult> ProcessRequest([FromRoute] string debtCode,
                                               [FromBody] IEnumerable<PostPaymentDto> payments,
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

        var addPayments = results.Where(x => x.Payment != null
                                           && x.Operation == EntityOperation.Add)
                                   .Select(x => x.Payment);

        unitOfWork.PaymentRepository.CreateCollection(addPayments);

        await unitOfWork.CompleteAsync();

        return Results.Ok(addPayments);
    }

    private async Task<ProcessPaymentResult> ProcessPayment(PostPaymentDto payment,
                                                            IUnitOfWork unitOfWork)
    {
        var charge = await unitOfWork.DebtDetailUserRepository.Find(payment.ChargeId);

        if (charge is null)
        {
            return new ProcessPaymentResult()
            {
                Success = false,
                Message = "ChargeId not found in the system.",
            };
        }

        var currentPayment = await unitOfWork.PaymentRepository.SearchBy(x => x.DebtDetailUser.Id == charge.Id);

        if (currentPayment.Any())
        {
            return new ProcessPaymentResult()
            {
                Success = false,
                Message = "There's already a payment for the specified ChargeId.",
            };
        }

        var newPayment = new Payment()
        {
            DebtDetailUser = charge,
            Status = PaymentStatus.Draft,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

        return new ProcessPaymentResult()
        {
            Success = true,
            Payment = newPayment,
            Operation = EntityOperation.Add
        };
    }
}
