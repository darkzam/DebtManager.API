using DebtManager.API.Configuration.Extensions;
using DebtManager.API.Filters;
using DebtManager.API.Models;
using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;

public class PostPaymentCollection : BaseEndpoint<Payment>
{
    public PostPaymentCollection(Uri baseRoute, WebApplication webApplication) : base(baseRoute, webApplication)
    { }

    public override void Initialize()
    {
        WebApplication.MapPost($"{BasePath.OriginalString}Payments", ProcessRequest)
                      .WithTags("Payments")
                      .AddEndpointFilter<AuthorizationFilter>()
                      .AddEndpointFilter<DebtValidatorFilter>()
                      .AddEndpointFilter<PostPaymentDtoValidatorFilter>();
    }

    private async Task<IResult> ProcessRequest([FromRoute] string debtCode,
                                               [FromBody] PostPaymentDto payments,
                                               IUnitOfWork unitOfWork,
                                               HttpContext context)
    {
        var debt = context.Items["debt"] as Debt;
        var cleanedPayments = context.Items["payments"] as PostPaymentDto;

        var result = await ProcessPayment(cleanedPayments, debt, unitOfWork);

        if (!result.Success)
        {
            return Results.BadRequest(result.Message);
        }

        var addedPayments = result.Payments.ToList();

        unitOfWork.PaymentRepository.CreateCollection(addedPayments);

        await unitOfWork.CompleteAsync();

        return Results.Ok(addedPayments);
    }

    private async Task<ProcessPaymentResult> ProcessPayment(PostPaymentDto payments,
                                                            Debt debt,
                                                            IUnitOfWork unitOfWork)
    {
        var allCharges = new List<DebtDetailUser>();
        if (payments.ChargeIds.HasItems())
        {
            var charges = await unitOfWork.DebtDetailUserRepository.SearchBy(x => payments.ChargeIds.Contains(x.Id));

            var difference = payments.ChargeIds.Except(charges.Select(x => x.Id));

            var sb = new StringBuilder();
            if (difference.Any())
            {
                sb.AppendJoin(", ", difference.Select(x => x.ToString()));

                return new ProcessPaymentResult()
                {
                    Success = false,
                    Message = $"ChargeIds not found in the system: [ {sb} ]",
                };
            }

            var currentPayments = await unitOfWork.PaymentRepository.SearchBy(x => charges.Select(x => x.Id).Contains(x.DebtDetailUser.Id));

            if (currentPayments.Any())
            {
                var paymentsDifference = charges.Select(x => x.Id)
                                                .Except(currentPayments.Select(x => x.DebtDetailUser.Id));

                sb.AppendJoin(", ", paymentsDifference.Select(x => x.ToString()));

                return new ProcessPaymentResult()
                {
                    Success = false,
                    Message = $"There are already payments for these specified ChargeIds: [ {sb} ].",
                };
            }

            allCharges.AddRange(charges);
        }

        if (payments.Usernames.HasItems())
        {
            var users = await unitOfWork.UserRepository.SearchBy(x => payments.Usernames.Contains(x.Username));

            var difference = payments.Usernames.Except(users.Select(x => x.Username));

            if (difference.Any())
            {
                var sb = new StringBuilder();
                sb.AppendJoin(", ", difference);

                return new ProcessPaymentResult()
                {
                    Success = false,
                    Message = $"Usernames not found in the system: [ {sb} ]",
                };
            }

            var userIds = users.Select(x => x.Id);

            var filteredCharges = await unitOfWork.DebtDetailUserRepository.SearchBy(x => x.DebtDetail.Debt.Id == debt.Id && userIds.Contains(x.User.Id));

            var filteredChargesIds = filteredCharges.Select(x => x.Id);

            var currentPayments = await unitOfWork.PaymentRepository.SearchBy(x => filteredChargesIds.Contains(x.DebtDetailUser.Id));

            var unpaidCharges = filteredCharges.Except(currentPayments.Select(x => x.DebtDetailUser));

            allCharges.AddRange(unpaidCharges);
        }

        var newPayments = allCharges.Distinct().Select(x => new Payment()
        {
            DebtDetailUser = x,
            Status = payments.PreApprovedPayments ? PaymentStatus.Approved : PaymentStatus.Draft,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        });

        return new ProcessPaymentResult()
        {
            Success = true,
            Payments = newPayments,
            Operation = EntityOperation.Add
        };
    }
}
