using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Lykke.Service.PlaceOrderBook.Middleware
{
    public sealed class XApiKeyAuthAttribute : ActionFilterAttribute
    {
        internal static IReadOnlyDictionary<string, string> Credentials { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue(ClientTokenMiddleware.ClientTokenHeader, out var h) ||
                h.Count != 1)
            {
                context.Result = new BadRequestObjectResult(
                    $"Header {ClientTokenMiddleware.ClientTokenHeader} with single value is required");
                return;
            }

            if (!Credentials.ContainsKey(h[0]))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
