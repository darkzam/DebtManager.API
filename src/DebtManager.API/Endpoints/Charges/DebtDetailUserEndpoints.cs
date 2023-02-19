﻿using DebtManager.Domain.Models;

public static class DebtDetailUserEndpoints
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

    private static IList<BaseEndpoint<DebtDetailUser>> RegisterEndpoints(WebApplication webApplication)
    {
        //To-Do implement a mechanism to provide routes for parent/children like routes.
        return new List<BaseEndpoint<DebtDetailUser>> { new PostDebtDetailUserCollection(new Uri($"{baseRoute}Debt/{{debtCode}}/", UriKind.Relative), webApplication),
                                                        new GetDebtDetailUserCollection(new Uri($"{baseRoute}Debt/{{debtCode}}/", UriKind.Relative), webApplication)};
    }
}
