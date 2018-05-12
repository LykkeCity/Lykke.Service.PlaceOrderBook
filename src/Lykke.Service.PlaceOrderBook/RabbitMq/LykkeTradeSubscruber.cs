using System;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.MatchingEngine.Connector.Messages;
using Lykke.MatchingEngine.Connector.Models;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.PlaceOrderBook.AzureRepositories;
using Lykke.Service.PlaceOrderBook.Settings.ServiceSettings;

namespace Lykke.Service.PlaceOrderBook.RabbitMq
{
    public class LykkeTradeSubscruber  : IStartable, IStopable
    {
        private readonly PlaceOrderBookSettings _settings;
        private readonly OrderRepository _orderRepository;
        private readonly ILog _log;
        private RabbitMqSubscriber<LimitOrderMessage> _subscriber;

        public LykkeTradeSubscruber(PlaceOrderBookSettings settings, OrderRepository orderRepository, ILog log)
        {
            _settings = settings;
            _orderRepository = orderRepository;
            _log = log;
        }

        public void Start()
        {
            var settings = new RabbitMqSubscriptionSettings
            {
                IsDurable = false,
                DeadLetterExchangeName = null,
                ExchangeName = _settings.LykkeTrade.Exchange,
                QueueName = $"{_settings.LykkeTrade.Exchange}.{_settings.LykkeTrade.QueueSuffix}",
                ConnectionString = _settings.LykkeTrade.ConnectionString
            };

            _subscriber = new RabbitMqSubscriber<LimitOrderMessage>(settings,
                    new ResilientErrorHandlingStrategy(_log, settings, TimeSpan.FromSeconds(10)))
                .SetMessageDeserializer(new JsonMessageDeserializer<LimitOrderMessage>())
                .SetMessageReadStrategy(new MessageReadQueueStrategy())
                .Subscribe(ProcessMessageAsync)
                .CreateDefaultBinding()
                .SetLogger(_log)
                .Start();
        }

        private async Task ProcessMessageAsync(LimitOrderMessage message)
        {
            foreach (var messageOrder in message.Orders)
            {
                if (!_settings.TrustedClientIds.Contains(messageOrder.Order.ClientId))
                    continue;
                
                if (messageOrder.Order.Status == OrderStatus.InOrderBook)
                {
                    var order = new OrderEntity()
                    {
                        ClientId = messageOrder.Order.ClientId,
                        OrderId = messageOrder.Order.ExternalId,
                        CreatedTime = DateTime.UtcNow,
                        AvgExecutionPrice = 0,
                        ExecutedAmount = 0,
                        Instrument = messageOrder.Order.AssetPairId,
                        OriginalAmount = Math.Abs(messageOrder.Order.Volume),
                        Price = messageOrder.Order.Price ?? 0,
                        TradeType = messageOrder.Order.Volume > 0 ? "Buy" : "Sell",
                        RemainingAmount = Math.Abs(messageOrder.Order.Volume),
                        Status = "Active",
                        CountExecute = 0,
                        SumExecutePrice = 0
                    };
                    await _orderRepository.AddOrReplace(order);
                }

                if (messageOrder.Order.Status == OrderStatus.Matched ||
                    messageOrder.Order.Status == OrderStatus.Processing)
                {
                    var order = await _orderRepository.GetOrder(messageOrder.Order.ClientId, messageOrder.Order.ExternalId);
                    if (order == null)
                    {
                        order = new OrderEntity()
                        {
                            ClientId = messageOrder.Order.ClientId,
                            OrderId = messageOrder.Order.ExternalId,
                            CreatedTime = DateTime.UtcNow,
                            AvgExecutionPrice = 0,
                            ExecutedAmount = 0,
                            Instrument = messageOrder.Order.AssetPairId,
                            OriginalAmount = Math.Abs(messageOrder.Order.Volume),
                            Price = messageOrder.Order.Price ?? 0,
                            TradeType = messageOrder.Order.Volume > 0 ? "Buy" : "Sell",
                            RemainingAmount = Math.Abs(messageOrder.Order.Volume),
                            Status = "Active",
                            CountExecute = 0,
                            SumExecutePrice = 0
                        };
                        await _orderRepository.AddOrReplace(order);
                    };

                    foreach (var trade in messageOrder.Trades)
                    {
                        var volume = messageOrder.Order.Volume < 0 ? trade.Volume : trade.OppositeVolume;
                        order.RemainingAmount -= Math.Abs(volume);
                        order.SumExecutePrice += trade.Price ?? 0;
                        order.CountExecute++;
                        order.AvgExecutionPrice = order.SumExecutePrice / order.CountExecute;
                        order.ExecutedAmount += Math.Abs(volume);

                        if (messageOrder.Order.Status == OrderStatus.Matched)
                            order.Status = "Fill";
                    }

                    await _orderRepository.AddOrReplace(order);
                }

                if (messageOrder.Order.Status == OrderStatus.Cancelled)
                {
                    var order = await _orderRepository.GetOrder(messageOrder.Order.ClientId, messageOrder.Order.ExternalId);
                    if (order == null) continue;

                    order.Status = "Canceled";
                    await _orderRepository.AddOrReplace(order);
                }

            }
        }

        public void Dispose()
        {
            _subscriber?.Dispose();
        }

        public void Stop()
        {
            _subscriber?.Stop();
        }
    }
}
