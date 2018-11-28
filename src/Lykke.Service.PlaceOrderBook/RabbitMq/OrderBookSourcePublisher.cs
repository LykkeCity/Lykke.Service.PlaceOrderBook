using System.Threading.Tasks;
using Autofac;
using Common;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.MatchingEngine.ExchangeModels;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.PlaceOrderBook.Core.Messaging;
using Lykke.Service.PlaceOrderBook.Settings.ServiceSettings;

namespace Lykke.Service.PlaceOrderBook.RabbitMq
{
    [UsedImplicitly]
    public class OrderBookSourcePublisher : IOrderBookSourcePublisher, IStartable, IStopable
    {
        private readonly OrderbookSourceSettings _orderBookSourceSettings;
        private readonly ILogFactory _logFactory;
        private RabbitMqPublisher<OrderBook> _rabbitPublisher;

        public OrderBookSourcePublisher(OrderbookSourceSettings orderBookSourceSettings, ILogFactory logFactory)
        {
            _orderBookSourceSettings = orderBookSourceSettings;
            _logFactory = logFactory;
        }

        public Task PublishAsync(OrderBook orderBook)
        {
            return _rabbitPublisher.ProduceAsync(orderBook);
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings.ForPublisher(_orderBookSourceSettings.ConnectionString,
                _orderBookSourceSettings.Exchange);

            _rabbitPublisher = new RabbitMqPublisher<OrderBook>(_logFactory, settings)
                .DisableInMemoryQueuePersistence()
                .SetSerializer(new GenericRabbitModelConverter<OrderBook>())
                .SetPublishStrategy(new DefaultFanoutPublishStrategy(settings))
                .PublishSynchronously()
                .Start();
        }

        public void Stop()
        {
            _rabbitPublisher?.Stop();
        }

        public void Dispose()
        {
            _rabbitPublisher?.Dispose();
        }
    }
}
