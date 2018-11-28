using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Common.Log;
using Lykke.Service.CryptoIndex.Contract;
using Lykke.Service.PlaceOrderBook.Core;
using Lykke.Service.PlaceOrderBook.Core.Messaging;

namespace Lykke.Service.PlaceOrderBook.RabbitMq.Publishers
{
    [UsedImplicitly]
    public class IndexTickPriceBatchPublisher : IIndexTickPriceBatchPublisher
    {
        private readonly TickPricePublisher _tickPricePublisher;
        private readonly IndexTickPricePublisher _indexTickPricePublisher;
        private readonly ILog _log;

        public IndexTickPriceBatchPublisher(
            TickPricePublisher tickPricePublisher,
            IndexTickPricePublisher indexTickPricePublisher,
            ILogFactory logFactory)
        {
            _tickPricePublisher = tickPricePublisher;
            _indexTickPricePublisher = indexTickPricePublisher;
            _log = logFactory.CreateLog(this);
        }

        public async Task PublishAsync(IndexTickPriceBatch tickPriceBatch)
        {
            foreach (TickPrice tickPrice in tickPriceBatch.TickPrices)
            {
                if (tickPrice.Timestamp == DateTime.MinValue)
                    tickPrice.Timestamp = DateTime.UtcNow;

                _log.Info("Publishing tick price.", tickPrice);

                await _tickPricePublisher.Publish(tickPrice);
            }

            foreach (IndexTickPrice indexTickPrice in tickPriceBatch.IndexTickPrices)
            {
                if (indexTickPrice.Timestamp == DateTime.MinValue)
                    indexTickPrice.Timestamp = DateTime.UtcNow;

                _log.Info("Publishing index tick price.", indexTickPrice);

                await _indexTickPricePublisher.Publish(indexTickPrice);
            }
        }
    }
}
