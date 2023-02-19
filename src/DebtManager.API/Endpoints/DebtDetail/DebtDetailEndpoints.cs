using DebtManager.Domain.Models;

public static class DebtDetailEndpoints
{
    public const string baseRoute = "api/";
    public static void Initialize(WebApplication webApplication)
    {
        var endpoints = RegisterEndpoints(webApplication);

        foreach (var endpoint in endpoints)
        {
            endpoint.Initialize();
        }
    }

    private static IList<BaseEndpoint<DebtDetail>> RegisterEndpoints(WebApplication webApplication)
    {
        //To-Do implement a mechanism to provide routes for parent/children like routes.
        return new List<BaseEndpoint<DebtDetail>> { new PostDebtDetailCollection(new Uri($"{baseRoute}Debt/{{debtCode}}/", UriKind.Relative), webApplication),
                                                    new PatchDebtDetailCollection(new Uri($"{baseRoute}Debt/{{debtCode}}/", UriKind.Relative), webApplication),
                                                    new GetDebtDetailCollection(new Uri($"{baseRoute}Debt/{{debtCode}}/", UriKind.Relative), webApplication)};
    }
}
