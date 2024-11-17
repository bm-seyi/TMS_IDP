using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Net;
using System.Threading.Tasks;

namespace TMS_API.Middleware
{
    public class ApiMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string apiKey;
        private const string API_HEADER = "x-api-key";
        
        public ApiMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            apiKey = configuration["API:Key"] ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue(API_HEADER, out StringValues extractedAPIKey) || string.IsNullOrWhiteSpace(extractedAPIKey))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized; //Unauthorized
                return;
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; // Internal Server Error
                return;
            }

            if (!string.Equals(apiKey, extractedAPIKey))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized; // Unauthorized
                return;
            }
            
            await _next(context);
        }

    }
}
