using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net;

namespace AuthenticationDeepDive.Weather.Basic.Middlewares
{
    public class BasicApiKeyAuthenticationHandler
    {
        private readonly RequestDelegate _next;
        private readonly string _apiKey;
        private readonly bool _enableAuthentication;
        private const string ApiKeyHeaderName = "x-api-key";

        public BasicApiKeyAuthenticationHandler(RequestDelegate next,
            IOptions<AuthenticationConfiguration> authenticationOptions)
        {
            _next = next;
            if (authenticationOptions == null)
            {
                throw new ArgumentNullException(nameof(authenticationOptions));
            }
            _apiKey = authenticationOptions.Value.ApiKey;
            _enableAuthentication = authenticationOptions.Value.Enable;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (_enableAuthentication)
            {
                if (!httpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var receivedApiKey))
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    var authenticationResponse = new AuthenticationErrorResponse
                    {
                        Code = "RE1001",
                        Message = "Api Key is missing in the request. Use {ApiKeyHeaderName}  as header param."
                    };
                    await httpContext.Response.WriteAsJsonAsync<AuthenticationErrorResponse>(authenticationResponse);
                    return;
                }
                if (!receivedApiKey.Equals(_apiKey))
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    var authenticationResponse = new AuthenticationErrorResponse
                    {
                        Code = "RE1002",
                        Message = "Invalid Api Key is sent in the request."
                    };
                    await httpContext.Response.WriteAsJsonAsync<AuthenticationErrorResponse>(authenticationResponse);
                    return;
                }
            }

            await _next(httpContext);

        }
    }
}
