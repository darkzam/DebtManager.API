namespace DebtManager.API.Filters
{
    public class DebtDetailGroupValidatorFilter : IEndpointFilter
    {
        public DebtDetailGroupValidatorFilter()
        { }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context,
                                                    EndpointFilterDelegate next)
        {
            var debtDetailGroups = context.GetArgument<IEnumerable<DebtDetailGroupDto>>(1);

            if (debtDetailGroups is null || !debtDetailGroups.Any())
            {
                return Results.BadRequest(nameof(debtDetailGroups));
            }

            var invalidModels = debtDetailGroups.Select(x => x.ValidateModel())
                                                .Where(x => !x.Success);

            if (invalidModels.Any())
            {
                return Results.BadRequest(invalidModels.Select(x => x.Message));
            }

            debtDetailGroups = debtDetailGroups.Select(x => new DebtDetailGroupDto()
            {
                ProductName = x.ProductName.Trim()
                                           .ToLower()
                                           .RemoveAccents(),
                Amount = x.Amount,
                Price = x.Price,
                Total = x.Total
            });

            var productPrices = debtDetailGroups.Select(x =>
                                                    new DebtDetailGroup()
                                                    {
                                                        ProductName = x.ProductName,
                                                        Price = (x.Price > 0) ? x.Price : (x.Total == 0 || x.Amount == 0) ? 0 : x.Total / x.Amount,
                                                        Amount = x.Amount,
                                                    })
                                                 .GroupBy(x => new
                                                 {
                                                     x.ProductName,
                                                     x.Price
                                                 });

            var multiplePricedProduct = productPrices.GroupBy(x => x.Key.ProductName)
                                                     .Where(group => group.Count() > 1);

            if (multiplePricedProduct.Any())
            {
                return Results.BadRequest($"Multiple different prices were submitted for '{multiplePricedProduct.First().Key}'");
            }

            var curatedProducts = productPrices.Select(group => new DebtDetailGroup()
            {
                ProductName = group.Key.ProductName,
                Amount = group.Sum(x => x.Amount),
                Price = group.Key.Price
            });

            context.HttpContext.Items.Add("productPrices", curatedProducts);

            return await next(context);
        }
    }
}
