using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.MatchingEngine.ExchangeModels;
using Lykke.Service.PlaceOrderBook.Core.Messaging;
using Lykke.Service.PlaceOrderBook.Core.Services;
using Lykke.Service.PlaceOrderBook.Core.Settings;
using Lykke.Service.RateCalculator.Client;
using Lykke.Service.RateCalculator.Client.AutorestClient.Models;

namespace Lykke.Service.PlaceOrderBook.Services
{
    [UsedImplicitly]
    public class OrderBookSourceService : IOrderBookSourceService
    {
        private static readonly TimeSpan PublishPeriod = TimeSpan.FromSeconds(1);
        private readonly ILog _log;
        private readonly IRateCalculatorClient _rateCalculatorClient;
        private readonly IOrderBookSourcePublisher _publisher;
        private readonly Timer _timer;
        private OrderBookSourceConfiguration _configuration;

        public OrderBookSourceService(
            IRateCalculatorClient rateCalculatorClient,
            IOrderBookSourcePublisher publisher,
            ILogFactory logFactory)
        {
            _rateCalculatorClient = rateCalculatorClient;
            _publisher = publisher;
            _log = logFactory.CreateLog(this);

            _timer = new Timer(OnTimer, null, PublishPeriod, Timeout.InfiniteTimeSpan);
        }

        public void Configure(OrderBookSourceConfiguration configuration)
        {
            _log.Info("Updating configuration", $"Configuration: {configuration.ToJson()}");
            _configuration = configuration;
        }

        private void OnTimer(object state)
        {
            try
            {
                GenerateOrderBooksAndPublish()
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception exception)
            {
                _log.Error(exception);
            }

            _timer.Change(PublishPeriod, Timeout.InfiniteTimeSpan);
        }

        private async Task GenerateOrderBooksAndPublish()
        {
            OrderBookSourceConfiguration config = _configuration;

            if (config == null)
                return;

            MarketProfile marketProfile = await _rateCalculatorClient.GetMarketProfileAsync();

            FeedData feedData = marketProfile?.Profile?.FirstOrDefault(p =>
                string.Equals(p.Asset, config.AssetPairId, StringComparison.InvariantCultureIgnoreCase));

            if (feedData == null)
            {
                _log.Warning("Could not get feed data", $"AssetPairId='{config.AssetPairId}'");
                return;
            }

            IReadOnlyList<OrderBook> orderBooks = OrderBookGenerator.GenerateOrderBooks(config.AssetPairId,
                feedData.Ask, feedData.Bid, config.Count, config.PriceDelta);

            foreach (OrderBook orderBook in orderBooks)
                await _publisher.PublishAsync(orderBook);
        }
    }
}
