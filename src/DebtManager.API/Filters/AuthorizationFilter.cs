using DebtManager.API.Models;
using Microsoft.Extensions.Options;

namespace DebtManager.API.Filters
{
    public class AuthorizationFilter : IEndpointFilter
    {
        private const string API_KEY = "api_key";
        private readonly AuthSettings _authSettings;

        public AuthorizationFilter(IOptions<AuthSettings> authSettings)
        {
            _authSettings = authSettings.Value;
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext filterContext, EndpointFilterDelegate next)
        {
            if (filterContext.HttpContext.Request.Headers.ContainsKey(API_KEY))
            {
                if (_authSettings.ApiKey == filterContext.HttpContext.Request.Headers[API_KEY].ToString())
                {
                    return await next(filterContext);
                }
            }

            return Results.Unauthorized();
        }
    }
}
