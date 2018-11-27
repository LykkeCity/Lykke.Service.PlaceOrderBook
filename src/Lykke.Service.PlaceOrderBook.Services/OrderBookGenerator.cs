using System;
using System.Collections.Generic;
using Lykke.MatchingEngine.ExchangeModels;

namespace Lykke.Service.PlaceOrderBook.Services
{
    public static class OrderBookGenerator
    {
        private const double Volume = 100;

        public static IReadOnlyList<OrderBook> GenerateOrderBooks(string assetPairId, double ask, double bid,
            int count, double priceDelta)
        {
            var list = new List<OrderBook>();

            for (var i = 0; i < count; i++)
            {
                var priceDeviation = (i % 2 == 0 ? -1 : 1) * priceDelta;

                var sellPrice = ask + priceDeviation >= 0 ? ask + priceDeviation : ask;
                var buyPrice = bid + priceDeviation >= 0 ? bid + priceDeviation : bid;

                // sell order book
                list.Add(new OrderBook
                {
                    AssetPair = assetPairId,
                    IsBuy = false,
                    Timestamp = DateTime.UtcNow,
                    Prices = new List<VolumePrice>
                    {
                        new VolumePrice
                        {
                            Price = sellPrice,
                            Volume = Volume
                        }
                    }
                });

                // buy order book
                list.Add(new OrderBook
                {
                    AssetPair = assetPairId,
                    IsBuy = true,
                    Timestamp = DateTime.UtcNow,
                    Prices = new List<VolumePrice>
                    {
                        new VolumePrice
                        {
                            Price = buyPrice,
                            Volume = Volume
                        }
                    }
                });
            }

            return list;
        }
    }
}
