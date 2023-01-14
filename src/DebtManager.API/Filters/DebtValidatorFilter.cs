using DebtManager.Application.Common.Interfaces;

namespace DebtManager.API.Filters
{
    public class DebtValidatorFilter : IEndpointFilter
    {
        private readonly IUnitOfWork _unitOfWork;
        public DebtValidatorFilter(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context,
                                                    EndpointFilterDelegate next)
        {
            var debtCode = context.GetArgument<string>(0);

            if (string.IsNullOrWhiteSpace(debtCode))
            {
                return Results.BadRequest($"Provided {nameof(debtCode)} is invalid.");
            }

            var debt = (await _unitOfWork.DebtRepository.SearchBy(x => x.Code == debtCode
                                                                                .Trim()
                                                                                .ToLower()
                                                                                .RemoveAccents()))
                                                        .FirstOrDefault();

            if (debt is null)
            {
                return Results.NotFound($"{nameof(debtCode)} provided does not exist in the system.");
            }

            context.HttpContext.Items.Add("debt", debt);

            return await next(context);
        }
    }
}
