using System;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.PlaceOrderBook.Core.Messaging;
using Lykke.Service.PlaceOrderBook.Settings.ServiceSettings;
using Newtonsoft.Json;

namespace Lykke.Service.PlaceOrderBook.RabbitMq
{
    [UsedImplicitly]
    public class OrderbookSourcePublisher : IOrderbookSourcePublisher, IStartable, IStopable
    {
        private readonly ILog _logger;
        private readonly RabbitMqPublisher<OrderBook> _rabbitPublisher;

        public OrderbookSourcePublisher(ILog logger, OrderbookSourceSettings settings)
        {
            _logger = logger;
            var publisherSettings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = settings.ConnectionString,
                ExchangeName = settings.Exchange,
                IsDurable = false
            };

            _rabbitPublisher = new RabbitMqPublisher<OrderBook>(publisherSettings)
                .DisableInMemoryQueuePersistence()
                .SetSerializer(new GenericRabbitModelConverter<OrderBook>())
                .SetLogger(logger)
                .SetPublishStrategy(new DefaultFanoutPublishStrategy(publisherSettings))
                .SetConsole(new ConsoleLWriter(Console.WriteLine))
                .PublishSynchronously();
        }

        public Task Publish(OrderBook message)
        {
            return _rabbitPublisher.ProduceAsync(message);
        }

        public void Start()
        {
            _rabbitPublisher.Start();
        }

        public void Stop()
        {
            _rabbitPublisher.Stop();
        }

        public void Dispose()
        {
            _rabbitPublisher.Dispose();
        }
    }
}
