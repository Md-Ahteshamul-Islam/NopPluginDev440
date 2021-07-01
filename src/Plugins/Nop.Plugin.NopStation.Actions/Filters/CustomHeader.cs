using Microsoft.AspNetCore.Mvc.Filters;

namespace Nop.Plugin.NopStation.Core.Filters
{
    public class CustomHeaderActionFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            context.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
        }
    }
}
