using System.Threading.Tasks;
using Autofac;
using Common;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.PlaceOrderBook.Settings.Rabbit;
using Lykke.Service.CryptoIndex.Contract;

namespace Lykke.Service.PlaceOrderBook.RabbitMq.Publishers
{
    [UsedImplicitly]
    public class IndexTickPricePublisher : IStartable, IStopable
    {
        private readonly PublisherSettings _publisherSettings;
        private readonly ILogFactory _logFactory;
        
        private RabbitMqPublisher<IndexTickPrice> _rabbitPublisher;

        public IndexTickPricePublisher(PublisherSettings publisherSettings, ILogFactory logFactory)
        {
            _publisherSettings = publisherSettings;
            _logFactory = logFactory;
        }

        public Task Publish(IndexTickPrice message)
        {
            return _rabbitPublisher.ProduceAsync(message);
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .ForPublisher(_publisherSettings.ConnectionString, _publisherSettings.Exchange);
            
            _rabbitPublisher = new RabbitMqPublisher<IndexTickPrice>(_logFactory, settings)
                .SetSerializer(new GenericRabbitModelConverter<IndexTickPrice>())
                .DisableInMemoryQueuePersistence()
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
