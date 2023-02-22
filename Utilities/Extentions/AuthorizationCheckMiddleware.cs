using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace TimesheetBE.Utilities.Extentions
{
    public class AuthorizationCheckMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthorizationCheckMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var features = httpContext.GetReverseProxyFeature();
            var policy = features.Route.Config.AuthorizationPolicy;
            Console.WriteLine($"Authorization policy: {policy}");
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                httpContext.Response.StatusCode = 401;
                httpContext.Response.ContentType = "application/json";
                await httpContext.Response.WriteAsync(JsonSerializer.Serialize(new { error = "Unauthorized", status = 401 }));
                return;
            }
            await _next(httpContext); // calling next middleware
        }
    }

    public static class AuthorizationCheckMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthorizationCheckMiddlewareMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthorizationCheckMiddleware>();
        }
    }
}