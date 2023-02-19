using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;

public class PostDebt : BaseEndpoint<Debt>
{
    public PostDebt(Uri baseRoute, WebApplication webApplication) : base(baseRoute, webApplication)
    { }

    public override void Initialize()
    {
        WebApplication.MapPost(Route.OriginalString, ProcessRequest)
                      .WithTags("Debt");
    }

    private async Task<IResult> ProcessRequest(DebtDto debtDto, IUnitOfWork unitOfWork)
    {
        if (debtDto is null)
        {
            return Results.BadRequest();
        }

        if (string.IsNullOrWhiteSpace(debtDto.Code) ||
            string.IsNullOrWhiteSpace(debtDto.Title) ||
            string.IsNullOrWhiteSpace(debtDto.Username) ||
            string.IsNullOrWhiteSpace(debtDto.BusinessName))
        {
            return Results.BadRequest();
        }

        if (debtDto.Total <= decimal.Zero)
        {
            return Results.BadRequest(nameof(debtDto.Total));
        }

        debtDto.Code = debtDto.Code.Trim()
                                   .ToLower()
                                   .RemoveAccents()
                                   .RemoveSpaces();

        var debt = await unitOfWork.DebtRepository.SearchBy(x => x.Code == debtDto.Code);

        if (debt.Any())
        {
            return Results.BadRequest($"DebtCode {debtDto.Code} is already in use");
        }

        debtDto.BusinessName = debtDto.BusinessName.Trim()
                                                   .ToLower()
                                                   .RemoveAccents();

        var business = (await unitOfWork.BusinessRepository.SearchBy(x => x.Name == debtDto.BusinessName))
                        .FirstOrDefault();

        if (business is null)
        {
            business = new Business()
            {
                Name = debtDto.BusinessName,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };
        }

        debtDto.Username = debtDto.Username.Trim()
                                           .ToLower()
                                           .RemoveAccents();

        var user = await unitOfWork.UserRepository.SearchBy(x => x.Username == debtDto.Username);

        var newDebt = new Debt
        {
            Code = debtDto.Code,
            Title = debtDto.Title,
            Total = debtDto.Total,
            ServiceRate = debtDto.ServiceRate,
            Host = user.FirstOrDefault(),
            Business = business,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

        var result = unitOfWork.DebtRepository.Create(newDebt);

        await unitOfWork.CompleteAsync();

        return Results.Ok(new DebtDto(result));
    }
}

