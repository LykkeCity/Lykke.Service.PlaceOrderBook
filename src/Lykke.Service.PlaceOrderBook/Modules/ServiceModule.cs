using System;
using System.Net;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage.Tables;
using Common;
using Common.Log;
using Lykke.MatchingEngine.Connector.Services;
using Lykke.Service.Balances.Client;
using Lykke.Service.PlaceOrderBook.AzureRepositories;
using Lykke.Service.PlaceOrderBook.Core.Messaging;
using Lykke.Service.PlaceOrderBook.Core.Services;
using Lykke.Service.PlaceOrderBook.RabbitMq;
using Lykke.Service.PlaceOrderBook.RabbitMq.Publishers;
using Lykke.Service.PlaceOrderBook.Settings.ServiceSettings;
using Lykke.Service.PlaceOrderBook.Services;
using Lykke.Service.PlaceOrderBook.Settings;
using Lykke.Service.RateCalculator.Client;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.PlaceOrderBook.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<PlaceOrderBookSettings> _settings;
        private readonly IReloadingManager<BalancesServiceClient> _balanceClientSetting;
        private readonly IReloadingManager<RateCalculatorServiceClient> _rateCalculatorClientSetting;

        private readonly ILog _log;
        // NOTE: you can remove it if you don't need to use IServiceCollection extensions to register service specific dependencies
        private readonly IServiceCollection _services;

        public ServiceModule
            (IReloadingManager<PlaceOrderBookSettings> settings, 
            IReloadingManager<BalancesServiceClient> balanceClientSetting,
            IReloadingManager<RateCalculatorServiceClient> rateCalculatorClientSetting,
            ILog log)
        {
            _settings = settings;
            _balanceClientSetting = balanceClientSetting;
            _rateCalculatorClientSetting = rateCalculatorClientSetting;
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

            builder
                .RegisterInstance<OrderRepository>(
                    new OrderRepository(
                        AzureTableStorage<OrderEntity>.Create(_settings.Nested(o => o.Db.DataConnString), "PlaceOrderSpotOrders", _log)))
                .SingleInstance();

            builder.RegisterType<OrderbookSourcePublisher>()
                .As<IStartable>()
                .As<IStopable>()
                .As<IOrderbookSourcePublisher>()
                .WithParameter("settings", _settings.CurrentValue.OrderbookSourceSettings)
                .AutoActivate()
                .SingleInstance();

            builder.RegisterType<IndexTickPricePublisher>()
                .As<IStartable>()
                .As<IStopable>()
                .AsSelf()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.Indices.IndexTickPriceExchange))
                .AutoActivate()
                .SingleInstance();

            builder.RegisterType<TickPricePublisher>()
                .As<IStartable>()
                .As<IStopable>()
                .AsSelf()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.Indices.TickPriceExchange))
                .AutoActivate()
                .SingleInstance();

            builder.RegisterType<IndexTickPriceBatchPublisher>()
                .As<IIndexTickPriceBatchPublisher>()
                .SingleInstance();

            builder.RegisterType<OrderbookSourceService>()
                .As<IOrderbookSourceService>()
                .SingleInstance();

            builder.RegisterBalancesClient(_balanceClientSetting.CurrentValue.ServiceUrl, _log);

            builder.BindMeClient(endPoint, socketLog);

            builder.RegisterRateCalculatorClient(_rateCalculatorClientSetting.CurrentValue.ServiceUrl, _log);

            builder.Populate(_services);
        }
    }
}
