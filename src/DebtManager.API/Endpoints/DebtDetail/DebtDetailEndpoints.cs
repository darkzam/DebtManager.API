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
        return new List<BaseEndpoint<DebtDetail>> { new PostDebtDetailCollection(new Uri(baseRoute, UriKind.Relative), webApplication) };
    }
}
