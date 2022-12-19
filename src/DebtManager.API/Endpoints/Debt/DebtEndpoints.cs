using DebtManager.Domain.Models;

public static class DebtEndpoints
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

    private static IList<BaseEndpoint<Debt>> RegisterEndpoints(WebApplication webApplication)
    {
        return new List<BaseEndpoint<Debt>> { new GetDebt(new Uri(baseRoute, UriKind.Relative), webApplication),
                                              new PostDebt(new Uri(baseRoute, UriKind.Relative), webApplication),
                                              new PutDebt(new Uri(baseRoute, UriKind.Relative), webApplication)};
    }
}
