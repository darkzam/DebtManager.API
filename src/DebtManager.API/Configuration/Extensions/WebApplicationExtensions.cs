﻿public static class WebApplicationExtensions
{
    public static void ConfigureApi(this WebApplication webApplication)
    {
        DebtEndpoints.Initialize(webApplication);
        DebtDetailEndpoints.Initialize(webApplication);
        DebtDetailUserEndpoints.Initialize(webApplication);
        PaymentEndpoints.Initialize(webApplication);
        //UserEndpoints.Initialize();
        //DebtUserEndpoints.Initialize();
        //DebtMetadataEndpoints.Initialize();
    }
}
