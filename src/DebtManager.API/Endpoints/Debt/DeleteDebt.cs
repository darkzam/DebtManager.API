using DebtManager.API.Filters;
using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;

public class DeleteDebt : BaseEndpoint<Debt>
{
    public DeleteDebt(Uri baseRoute, WebApplication webApplication) : base(baseRoute, webApplication)
    { }

    public override void Initialize()
    {
        WebApplication.MapDelete($"{Route.OriginalString}/{{id}}", ProcessRequest)
                      .WithTags("Debts")
                      .AddEndpointFilter<AuthorizationFilter>();
    }

    private async Task<IResult> ProcessRequest(Guid id, IUnitOfWork unitOfWork)
    {
        var debt = await unitOfWork.DebtRepository.Find(id);

        if(debt is null)
        {
            return Results.NotFound();
        }

        unitOfWork.DebtRepository.Remove(debt);

        await unitOfWork.CompleteAsync();

        return Results.Ok();
    }
}
