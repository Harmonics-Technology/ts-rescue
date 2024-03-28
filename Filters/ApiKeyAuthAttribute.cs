using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace TimesheetBE.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
    {
        private const string ApiKeyHeaderName = "ApiKey";
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var value))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var apikey = "4Shewv2dogWI1aR";

            if (!apikey.Equals(value))
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            await next();
        }
    }
}
