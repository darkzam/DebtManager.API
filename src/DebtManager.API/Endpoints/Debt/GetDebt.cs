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
        Route = new Uri(BasePath, Action);
        HttpVerb = HttpVerb.Get;
        _webApplication = webApplication ?? throw new ArgumentNullException(nameof(webApplication));
    }

    public override void Initialize()
    {
        _webApplication.Map(Route.AbsoluteUri, ProcessRequest);
    }

    private async Task<IResult> ProcessRequest(IDebtRepository debtRepository)
    {
        var result = await debtRepository.GetAll();

        return Results.Ok(result);
    }
}
