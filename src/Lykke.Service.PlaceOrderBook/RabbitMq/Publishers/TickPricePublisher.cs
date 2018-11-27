using System.Threading.Tasks;
using Autofac;
using Common;
using JetBrains.Annotations;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.PlaceOrderBook.Settings.Rabbit;

namespace Lykke.Service.PlaceOrderBook.RabbitMq.Publishers
{
    [UsedImplicitly]
    public class TickPricePublisher : IStartable, IStopable
    {
        private readonly PublisherSettings _publisherSettings;
        private readonly ILogFactory _logFactory;

        private RabbitMqPublisher<TickPrice> _publisher;

        public TickPricePublisher(PublisherSettings publisherSettings, ILogFactory logFactory)
        {
            _publisherSettings = publisherSettings;
            _logFactory = logFactory;
        }

        public Task Publish(TickPrice message)
        {
            return _publisher.ProduceAsync(message);
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .ForPublisher(_publisherSettings.ConnectionString, _publisherSettings.Exchange);

            _publisher = new RabbitMqPublisher<TickPrice>(_logFactory, settings)
                .DisableInMemoryQueuePersistence()
                .SetSerializer(new GenericRabbitModelConverter<TickPrice>())
                .SetPublishStrategy(new DefaultFanoutPublishStrategy(settings))
                .PublishSynchronously()
                .Start();
        }

        public void Stop()
        {
            _publisher?.Stop();
        }

        public void Dispose()
        {
            _publisher?.Dispose();
        }
    }
}
