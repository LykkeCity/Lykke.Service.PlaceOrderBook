using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PlaceOrderBook.Middleware
{
    public static class ClientTokenMiddleware
    {
        private const string CredsKey = "api-credentials";

        public static string ClientId(this Controller controller)
        {
            if (controller.HttpContext.Items.TryGetValue(CredsKey, out var creds))
            {
                return (string) creds;
            }

            return null;
        }

        public const string ClientTokenHeader = "X-API-KEY";

        public static void UseAuthenticationMiddleware(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                if (context.Request.Headers.TryGetValue(ClientTokenHeader, out var token))
                {
                    var clientId = token.FirstOrDefault();
                    if (!string.IsNullOrEmpty(clientId))
                        context.Items[CredsKey] = clientId;
                }
                else
                {
                    context.Items[CredsKey] = "";
                }

                await next();
            });
        }
    }
}
