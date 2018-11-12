using System;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.PlaceOrderBook.Settings.Rabbit;

namespace Lykke.Service.PlaceOrderBook.RabbitMq.Publishers
{
    [UsedImplicitly]
    public class TickPricePublisher : IStartable, IStopable
    {
        private readonly RabbitMqPublisher<TickPrice> _publisher;

        public TickPricePublisher(ILog logger, PublisherSettings settings)
        {
            var publisherSettings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = settings.ConnectionString,
                ExchangeName = settings.Exchange,
                IsDurable = settings.IsDurable
            };

            _publisher = new RabbitMqPublisher<TickPrice>(publisherSettings)
                .DisableInMemoryQueuePersistence()
                .SetSerializer(new GenericRabbitModelConverter<TickPrice>())
                .SetLogger(logger)
                .SetPublishStrategy(new DefaultFanoutPublishStrategy(publisherSettings))
                .PublishSynchronously();
        }

        public Task Publish(TickPrice message)
        {
            return _publisher.ProduceAsync(message);
        }

        public void Start()
        {
            _publisher.Start();
        }

        public void Stop()
        {
            _publisher.Stop();
        }

        public void Dispose()
        {
            _publisher.Dispose();
        }
    }
}
