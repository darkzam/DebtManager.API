using DebtManager.API.Filters;
using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;

public class PutDebt : BaseEndpoint<Debt>
{
    public PutDebt(Uri baseRoute, WebApplication webApplication) : base(baseRoute, webApplication)
    { }

    public override void Initialize()
    {
        WebApplication.MapPut(Route.OriginalString, ProcessRequest)
                      .WithTags("Debts")
                      .AddEndpointFilter<AuthorizationFilter>();
    }

    private async Task<IResult> ProcessRequest(DebtDto debtDto, IUnitOfWork unitOfWork)
    {
        if (debtDto is null)
        {
            return Results.BadRequest();
        }

        if (debtDto.Id.Equals(Guid.Empty))
        {
            return Results.BadRequest($"Provide a non-empty {nameof(debtDto.Id)}");
        }

        var debt = await unitOfWork.DebtRepository.Find(debtDto.Id);

        if (debt is null)
        {
            return Results.NotFound("A Debt with provided Id was not found in the system.");
        }

        //Todo-Clean string fields before input Code, Title
        // get rid of strange chars: accents, spaces, etc

        if (debtDto.Total <= decimal.Zero)
        {
            return Results.BadRequest(nameof(debtDto.Total));
        }

        var user = await unitOfWork.UserRepository.SearchBy(x => x.Username == debtDto.Username);

        debt.Host = user.FirstOrDefault();
        debt.Code = debtDto.Code.Trim();
        debt.Title = debtDto.Title.Trim();
        debt.Total = debtDto.Total;
        debt.ServiceRate = debtDto.ServiceRate;
        debt.UpdatedDate = DateTime.Now;

        unitOfWork.DebtRepository.Update(debt);

        await unitOfWork.CompleteAsync();

        return Results.Ok(new DebtDto(debt));
    }
}
