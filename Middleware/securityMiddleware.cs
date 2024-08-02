using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace TMS_API.Middleware;

public class ApiMiddleware
{
    private readonly RequestDelegate _next;
    private const string API_HEADER = "X-API-KEY";
    
    public ApiMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(API_HEADER, out var extractedAPIKey) || string.IsNullOrWhiteSpace(extractedAPIKey))
        {
            context.Response.StatusCode = 401; //Unauthorized
            return;
        }

        string? apiKey =  Environment.GetEnvironmentVariable("Key");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            context.Response.StatusCode = 500; // Internal Server Error
            return;
        }

        if (!string.Equals(apiKey, extractedAPIKey))
        {
            context.Response.StatusCode = 403; // Forbidden
            return;
        }


        await _next(context);
    }

}