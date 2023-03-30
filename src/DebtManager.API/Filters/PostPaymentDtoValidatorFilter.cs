using DebtManager.API.Configuration.Extensions;
using DebtManager.API.Models;

namespace DebtManager.API.Filters
{
    public class PostPaymentDtoValidatorFilter : IEndpointFilter
    {
        public PostPaymentDtoValidatorFilter()
        { }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context,
                                                    EndpointFilterDelegate next)
        {
            var postPaymentDto = context.GetArgument<PostPaymentDto>(1);

            if (postPaymentDto is null)
            {
                return Results.BadRequest(nameof(postPaymentDto));
            }

            var validationResult = postPaymentDto.ValidateModel();

            if (!validationResult.Success)
            {
                return Results.BadRequest(validationResult.Message);
            }

            if (postPaymentDto.ChargeIds.HasItems())
            {
                postPaymentDto.ChargeIds = postPaymentDto.ChargeIds.Distinct();
            }

            if (postPaymentDto.Usernames.HasItems())
            {
                postPaymentDto.Usernames = postPaymentDto.Usernames.Select(x => x.Trim()
                                                                                 .ToLower()
                                                                                 .RemoveSpaces())
                                                                    .Distinct();
            }

            context.HttpContext.Items.Add("payments", postPaymentDto);

            return await next(context);
        }
    }
}
