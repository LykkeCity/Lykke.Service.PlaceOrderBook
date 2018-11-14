using System;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.PlaceOrderBook.Settings.Rabbit;
using Lykke.Service.CryptoIndex.Contract;

namespace Lykke.Service.PlaceOrderBook.RabbitMq.Publishers
{
    [UsedImplicitly]
    public class IndexTickPricePublisher : IStartable, IStopable
    {
        private readonly ILog _logger;
        private readonly RabbitMqPublisher<IndexTickPrice> _rabbitPublisher;

        public IndexTickPricePublisher(ILog logger, PublisherSettings settings)
        {
            _logger = logger;
            var publisherSettings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = settings.ConnectionString,
                ExchangeName = settings.Exchange,
                IsDurable = settings.IsDurable
            };

            _rabbitPublisher = new RabbitMqPublisher<IndexTickPrice>(publisherSettings)
                .DisableInMemoryQueuePersistence()
                .SetSerializer(new GenericRabbitModelConverter<IndexTickPrice>())
                .SetLogger(logger)
                .SetPublishStrategy(new DefaultFanoutPublishStrategy(publisherSettings))
                .PublishSynchronously();
        }

        public Task Publish(IndexTickPrice message)
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
