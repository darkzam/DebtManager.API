using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;

public class PostDebt : BaseEndpoint<Debt>
{
    public PostDebt(Uri baseRoute, WebApplication webApplication) : base(baseRoute, webApplication)
    { }

    public override void Initialize()
    {
        WebApplication.MapPost(Route.OriginalString, ProcessRequest);
    }

    private async Task<IResult> ProcessRequest(DebtDto debtDto, IUnitOfWork unitOfWork)
    {
        if (debtDto is null)
        {
            return Results.BadRequest();
        }

        if (string.IsNullOrWhiteSpace(debtDto.Code) ||
            string.IsNullOrWhiteSpace(debtDto.Title))
        {
            return Results.BadRequest();
        }

        if (debtDto.Total.Equals(decimal.Zero))
        {
            return Results.BadRequest(nameof(debtDto.Total));
        }

        var user = await unitOfWork.UserRepository.Find(debtDto.HostId);

        var newDebt = new Debt
        {
            Code = debtDto.Code,
            Title = debtDto.Title,
            Total = debtDto.Total,
            ServiceRate = debtDto.ServiceRate,
            Host = user,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

        var result = unitOfWork.DebtRepository.Create(newDebt);

        await unitOfWork.CompleteAsync();

        return Results.Ok(result);
    }
}

