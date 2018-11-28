using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Common;
using Lykke.Common.Log;
using Lykke.MatchingEngine.Connector.Messages;
using Lykke.MatchingEngine.Connector.Models;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.PlaceOrderBook.AzureRepositories.Orders;
using Lykke.Service.PlaceOrderBook.Settings.ServiceSettings;

namespace Lykke.Service.PlaceOrderBook.RabbitMq
{
    public class LykkeTradeSubscriber : IStartable, IStopable
    {
        private readonly ExchangeSettings _exchangeSettings;
        private readonly IReadOnlyCollection<string> _trustedClients;
        private readonly OrderRepository _orderRepository;
        private readonly ILogFactory _logFactory;
        private RabbitMqSubscriber<LimitOrderMessage> _subscriber;

        public LykkeTradeSubscriber(
            ExchangeSettings exchangeSettings,
            IReadOnlyCollection<string> trustedClients,
            OrderRepository orderRepository,
            ILogFactory logFactory)
        {
            _exchangeSettings = exchangeSettings;
            _trustedClients = trustedClients;
            _orderRepository = orderRepository;
            _logFactory = logFactory;
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings.ForSubscriber(_exchangeSettings.ConnectionString,
                _exchangeSettings.Exchange, _exchangeSettings.QueueSuffix);

            _subscriber = new RabbitMqSubscriber<LimitOrderMessage>(_logFactory, settings,
                    new ResilientErrorHandlingStrategy(_logFactory, settings, TimeSpan.FromSeconds(10)))
                .SetMessageDeserializer(new JsonMessageDeserializer<LimitOrderMessage>())
                .SetMessageReadStrategy(new MessageReadQueueStrategy())
                .Subscribe(ProcessMessageAsync)
                .CreateDefaultBinding()
                .Start();
        }

        private async Task ProcessMessageAsync(LimitOrderMessage message)
        {
            foreach (var messageOrder in message.Orders)
            {
                if (!_trustedClients.Contains(messageOrder.Order.ClientId))
                    continue;

                if (messageOrder.Order.Status == OrderStatus.InOrderBook)
                {
                    var order = new OrderEntity
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
                    var order = await _orderRepository.GetOrder(messageOrder.Order.ClientId,
                        messageOrder.Order.ExternalId);
                    if (order == null)
                    {
                        order = new OrderEntity
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
                    var order = await _orderRepository.GetOrder(messageOrder.Order.ClientId,
                        messageOrder.Order.ExternalId);
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
