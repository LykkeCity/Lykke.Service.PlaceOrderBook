using System;
using System.Collections.Generic;
using Lykke.Common.ExchangeAdapter.Contracts;

namespace Lykke.Service.PlaceOrderBook.Core.Domain.Simulation
{
    public class OrderbookGenerator
    {
        private const decimal Volume = 100;

        public IReadOnlyList<OrderBook> GenerateOrderBooks(string source, string assetPairId, double ask, double bid, 
            int count, double priceDelta)
        {
            var list = new List<OrderBook>();

            for (var i = 0; i < count; i++)
            {
                var priceDeviation = (i % 2 == 0 ? -1 : 1) * priceDelta;

                var sellPrice = ask + priceDeviation >= 0 ? ask + priceDeviation : ask;
                var buyPrice =  bid + priceDeviation >= 0 ? bid + priceDeviation : bid;

                // sell orderbook

                list.Add(new OrderBook(
                    source,
                    assetPairId,
                    DateTime.UtcNow,
                    new[] // asks
                    {
                        new OrderBookItem((decimal)sellPrice, Volume)
                    },
                    new OrderBookItem[0] // bids
                ));

                // buy orderbook

                list.Add(new OrderBook(
                    source,
                    assetPairId,
                    DateTime.UtcNow,
                    new OrderBookItem[0], // asks
                    new [] // bids
                    {
                        new OrderBookItem((decimal)buyPrice, Volume)
                    }
                ));
            }

            return list;
        }
    }
}
