using DebtManager.Domain.Models;

namespace DebtManager.API.Endpoints.Reports
{
    public static class ReportsEndpoints
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
            return new List<BaseEndpoint<DebtDetailUser>> { new GetReportDebts(new Uri($"{baseRoute}Reports/", UriKind.Relative), webApplication) };
        }
    }
}
