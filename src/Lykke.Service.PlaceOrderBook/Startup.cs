using System;
using System.Linq;
using AutoMapper;
using JetBrains.Annotations;
using Lykke.Sdk;
using Lykke.Service.PlaceOrderBook.Middleware;
using Lykke.Service.PlaceOrderBook.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.PlaceOrderBook
{
    public class Startup
    {
        private readonly LykkeSwaggerOptions _swaggerOptions = new LykkeSwaggerOptions
        {
            ApiTitle = "IndexHedgingEngine API",
            ApiVersion = "v1"
        };

        [UsedImplicitly]
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return services.BuildServiceProvider<AppSettings>(options =>
            {
                options.Extend = (serviceCollection, settings) =>
                {
                    Mapper.Initialize(cfg => { cfg.AddProfiles(typeof(AutoMapperProfile)); });

                    Mapper.AssertConfigurationIsValid();

                    XApiKeyAuthAttribute.Credentials = settings.CurrentValue
                        .PlaceOrderBookService
                        .TrustedClientIds
                        .ToDictionary(e => e, e => e);
                };

                options.Swagger = swagger => { swagger.OperationFilter<AddLykkeAuthorizationHeaderFilter>(); };
                options.SwaggerOptions = _swaggerOptions;

                options.Logs = logs =>
                {
                    logs.AzureTableName = "PlaceOrderBookLog";
                    logs.AzureTableConnectionStringResolver = settings =>
                        settings.PlaceOrderBookService.Db.LogsConnString;
                };
            });
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app)
        {
            app.UseLykkeConfiguration(options =>
            {
                options.SwaggerOptions = _swaggerOptions;

                options.WithMiddleware = x => { x.UseAuthenticationMiddleware(); };
            });
        }
    }
}
