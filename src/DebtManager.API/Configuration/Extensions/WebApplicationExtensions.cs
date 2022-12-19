public static class WebApplicationExtensions
{
    public static void ConfigureApi(this WebApplication webApplication)
    {
        DebtEndpoints.Initialize(webApplication);
        //UserEndpoints.Initialize();
        //DebtUserEndpoints.Initialize();
        //DebtMetadataEndpoints.Initialize();
    }
}
