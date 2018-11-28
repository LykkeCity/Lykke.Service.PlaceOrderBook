using System;
using System.ComponentModel.DataAnnotations;
using Lykke.Service.PlaceOrderBook.Core.Settings;

namespace Lykke.Service.PlaceOrderBook.Models
{
    public class OrderbookSourceConfigurationViewModel
    {
        [Required]
        public string Source { get; set; }

        [Required]
        public string AssetPairId { get; set; }

        [Required]
        [Range(0, Int32.MaxValue)]
        public int Count { get; set; }

        [Required]
        [Range(0, Int32.MaxValue)]
        public double PriceDelta { get; set; }

        public OrderBookSourceConfiguration ToModel()
        {
            return new OrderBookSourceConfiguration(Source, AssetPairId, Count, PriceDelta);
        }
    }
}
