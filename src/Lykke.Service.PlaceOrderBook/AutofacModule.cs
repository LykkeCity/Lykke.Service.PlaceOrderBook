using System;
using System.Collections.Generic;
using System.Net;
using Autofac;
using AzureStorage.Tables;
using Common;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.MatchingEngine.Connector.Services;
using Lykke.Service.Balances.Client;
using Lykke.Service.PlaceOrderBook.AzureRepositories.Orders;
using Lykke.Service.PlaceOrderBook.Core.Messaging;
using Lykke.Service.PlaceOrderBook.Core.Services;
using Lykke.Service.PlaceOrderBook.RabbitMq;
using Lykke.Service.PlaceOrderBook.RabbitMq.Publishers;
using Lykke.Service.PlaceOrderBook.Services;
using Lykke.Service.PlaceOrderBook.Settings;
using Lykke.Service.PlaceOrderBook.Settings.Clients.MatchingEngine;
using Lykke.Service.RateCalculator.Client;
using Lykke.SettingsReader;

namespace Lykke.Service.PlaceOrderBook
{
    [UsedImplicitly]
    public class AutofacModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;

        public AutofacModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(container => new OrderRepository(
                    AzureTableStorage<OrderEntity>.Create(
                        _settings.Nested(o => o.PlaceOrderBookService.Db.DataConnString),
                        "PlaceOrderSpotOrders", container.Resolve<ILogFactory>())))
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<IndexTickPriceBatchPublisher>()
                .As<IIndexTickPriceBatchPublisher>()
                .SingleInstance();

            builder.RegisterType<OrderBookSourceService>()
                .As<IOrderBookSourceService>()
                .SingleInstance();

            builder.RegisterRateCalculatorClient(_settings.CurrentValue.RateCalculatorServiceClient.ServiceUrl);

            RegisterRabbit(builder);

            RegisterClients(builder);
        }

        private void RegisterRabbit(ContainerBuilder builder)
        {
            builder.RegisterType<LykkeTradeSubscriber>()
                .As<IStartable>()
                .As<IStopable>()
                .WithParameter(
                    TypedParameter.From(_settings.CurrentValue.PlaceOrderBookService.LykkeTrade))
                .WithParameter(
                    TypedParameter.From<IReadOnlyCollection<string>>(_settings.CurrentValue.PlaceOrderBookService
                        .TrustedClientIds))
                .AutoActivate()
                .SingleInstance();

            builder.RegisterType<OrderBookSourcePublisher>()
                .As<IStartable>()
                .As<IStopable>()
                .As<IOrderBookSourcePublisher>()
                .WithParameter(
                    TypedParameter.From(_settings.CurrentValue.PlaceOrderBookService.OrderbookSourceSettings))
                .AutoActivate()
                .SingleInstance();

            builder.RegisterType<IndexTickPricePublisher>()
                .As<IStartable>()
                .As<IStopable>()
                .AsSelf()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.PlaceOrderBookService.Indices
                    .IndexTickPriceExchange))
                .AutoActivate()
                .SingleInstance();

            builder.RegisterType<TickPricePublisher>()
                .As<IStartable>()
                .As<IStopable>()
                .AsSelf()
                .WithParameter(
                    TypedParameter.From(_settings.CurrentValue.PlaceOrderBookService.Indices.TickPriceExchange))
                .AutoActivate()
                .SingleInstance();
        }

        private void RegisterClients(ContainerBuilder builder)
        {
            builder.RegisterBalancesClient(_settings.CurrentValue.BalancesServiceClient);

            MatchingEngineClientSettings matchingEngineClientSettings =
                _settings.CurrentValue.PlaceOrderBookService.MatchingEngine;

            if (!IPAddress.TryParse(matchingEngineClientSettings.IpEndpoint.Host, out var address))
                address = Dns.GetHostAddressesAsync(matchingEngineClientSettings.IpEndpoint.Host).Result[0];

            var socketLog = new SocketLogDynamic(i => { },
                str => Console.WriteLine(DateTime.UtcNow.ToIsoDateTime() + ": " + str));

            var endPoint = new IPEndPoint(address, matchingEngineClientSettings.IpEndpoint.Port);

            builder.BindMeClient(endPoint, socketLog);
        }
    }
}
