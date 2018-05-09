using System;
using System.Net;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common;
using Common.Log;
using Lykke.MatchingEngine.Connector.Services;
using Lykke.Service.PlaceOrderBook.Core.Services;
using Lykke.Service.PlaceOrderBook.Settings.ServiceSettings;
using Lykke.Service.PlaceOrderBook.Services;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.PlaceOrderBook.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<PlaceOrderBookSettings> _settings;
        private readonly ILog _log;
        // NOTE: you can remove it if you don't need to use IServiceCollection extensions to register service specific dependencies
        private readonly IServiceCollection _services;

        public ServiceModule(IReloadingManager<PlaceOrderBookSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            // TODO: Do not register entire settings in container, pass necessary settings to services which requires them
            // ex:
            //  builder.RegisterType<QuotesPublisher>()
            //      .As<IQuotesPublisher>()
            //      .WithParameter(TypedParameter.From(_settings.CurrentValue.QuotesPublication))

            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();

            var socketLog = new SocketLogDynamic(i => { },
                str => Console.WriteLine(DateTime.UtcNow.ToIsoDateTime() + ": " + str));

            if (!IPAddress.TryParse(_settings.CurrentValue.MatchingEngine.IpEndpoint.Host, out var address))
                address = Dns.GetHostAddressesAsync(_settings.CurrentValue.MatchingEngine.IpEndpoint.Host).Result[0];

            var endPoint = new IPEndPoint(address, _settings.CurrentValue.MatchingEngine.IpEndpoint.Port);

            builder.BindMeClient(endPoint, socketLog);

            builder.Populate(_services);
        }
    }
}
