using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.ExchangeAdapter.Contracts;
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
            ILog log)
        {
            _tickPricePublisher = tickPricePublisher;
            _indexTickPricePublisher = indexTickPricePublisher;
            _log = log.CreateComponentScope(nameof(IndexTickPricePublisher));
        }

        public async Task Publish(IndexTickPriceBatch tickPriceBatch)
        {
            foreach (TickPrice tickPrice in tickPriceBatch.TickPrices)
            {
                if (tickPrice.Timestamp == DateTime.MinValue)
                {
                    tickPrice.Timestamp = DateTime.UtcNow;
                }

                await _log.WriteInfoAsync(nameof(Publish), tickPrice.ToJson(), "Publishing tick price.");

                await _tickPricePublisher.Publish(tickPrice);
            }

            foreach (IndexTickPrice indexTickPrice in tickPriceBatch.IndexTickPrices)
            {
                if (indexTickPrice.Timestamp == DateTime.MinValue)
                {
                    indexTickPrice.Timestamp = DateTime.UtcNow;
                }

                await _log.WriteInfoAsync(nameof(Publish), indexTickPrice.ToJson(), "Publishing index tick price.");

                await _indexTickPricePublisher.Publish(indexTickPrice);
            }
        }
    }
}
