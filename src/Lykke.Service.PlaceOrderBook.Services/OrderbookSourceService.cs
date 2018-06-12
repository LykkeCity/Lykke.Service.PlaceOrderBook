using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Service.PlaceOrderBook.Core.Domain.Simulation;
using Lykke.Service.PlaceOrderBook.Core.Messaging;
using Lykke.Service.PlaceOrderBook.Core.Services;
using Lykke.Service.PlaceOrderBook.Core.Settings;
using Lykke.Service.RateCalculator.Client;
using MoreLinq;

namespace Lykke.Service.PlaceOrderBook.Services
{
    [UsedImplicitly]
    public class OrderbookSourceService : IOrderbookSourceService
    {
        private static readonly TimeSpan PublishPeriod = TimeSpan.FromSeconds(1);
        private readonly ILog _log;
        private readonly IRateCalculatorClient _rateCalculatorClient;
        private readonly IOrderbookSourcePublisher _publisher;
        private readonly OrderbookGenerator _generator;
        private readonly Timer _timer;
        private OrderbookSourceConfiguration _configuration;

        public OrderbookSourceService(IRateCalculatorClient rateCalculatorClient, IOrderbookSourcePublisher publisher, ILog log)
        {
            _rateCalculatorClient = rateCalculatorClient;
            _publisher = publisher;
            _generator = new OrderbookGenerator();
            _log = log.CreateComponentScope(nameof(OrderbookSourceService));
            _timer = new Timer(OnTimer, null, PublishPeriod, Timeout.InfiniteTimeSpan);
        }

        public void Configure(OrderbookSourceConfiguration configuration)
        {
            _log.WriteInfoAsync(nameof(Configure), $"Configuration: {configuration.ToJson()}",
                "Updating configuration");
            _configuration = configuration;
        }

        private void OnTimer(object state)
        {
            try
            {
                GenerateOrderbooksAndPublish().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _log.WriteErrorAsync(nameof(OnTimer), string.Empty, ex);
            }

            _timer.Change(PublishPeriod, Timeout.InfiniteTimeSpan);
        }

        private async Task GenerateOrderbooksAndPublish()
        {
            var config = _configuration;
            if (config == null)
            {
                return;
            }

            var market = await _rateCalculatorClient.GetMarketProfileAsync();
            var feed = market?.Profile?.FirstOrDefault(p =>
                string.Equals(p.Asset, config.AssetPairId, StringComparison.InvariantCultureIgnoreCase));

            if (feed == null)
            {
                await _log.WriteWarningAsync(nameof(GenerateOrderbooksAndPublish), $"AssetPairId='{config.AssetPairId}'",
                    "Could not get feed data");
                return;
            }

            var orderboks = _generator.GenerateOrderBooks(config.AssetPairId, feed.Ask, feed.Bid, config.Count, config.PriceDelta);

            orderboks.ForEach(ob => _publisher.Publish(ob).GetAwaiter().GetResult());
        }
    }
}
