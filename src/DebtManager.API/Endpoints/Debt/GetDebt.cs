using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;

public class GetDebt : BaseEndpoint<Debt>
{
    public GetDebt(Uri baseRoute, WebApplication webApplication) : base(baseRoute, webApplication)
    { }

    public override void Initialize()
    {
        WebApplication.MapGet(Route.OriginalString, ProcessRequest);
    }

    private async Task<IResult> ProcessRequest(IUnitOfWork unitOfWork)
    {
        var result = await unitOfWork.DebtRepository.GetAll();

        return Results.Ok(result.Select(x => new DebtDto(x)));
    }
}
