using DebtManager.Application.Common.Interfaces;
using DebtManager.Domain.Models;

public class GetDebt : BaseEndpoint
{
    private readonly WebApplication _webApplication;
    public GetDebt(Uri baseRoute,
                   WebApplication webApplication)
    {
        BasePath = baseRoute ?? throw new ArgumentNullException(nameof(baseRoute));
        Action = new Uri(nameof(Debt), UriKind.Relative);
        Route = new Uri($"{BasePath.ToString()}{Action.ToString()}", UriKind.Relative);
        HttpVerb = HttpVerb.Get;
        _webApplication = webApplication ?? throw new ArgumentNullException(nameof(webApplication));
    }

    public override void Initialize()
    {
        _webApplication.Map(Route.OriginalString, ProcessRequest);
    }

    private async Task<IResult> ProcessRequest(IUnitOfWork unitOfWork)
    {
        var result = await unitOfWork.DebtRepository.GetAll();

        return Results.Ok(result);
    }
}
