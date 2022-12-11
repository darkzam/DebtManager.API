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

    private static IList<BaseEndpoint> RegisterEndpoints(WebApplication webApplication)
    {
        return new List<BaseEndpoint> { new GetDebt(new Uri(baseRoute), webApplication) };
    }
}
