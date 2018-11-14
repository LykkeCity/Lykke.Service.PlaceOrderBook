using System;
using JetBrains.Annotations;

namespace Lykke.Service.PlaceOrderBook.Client.Models.IndexTickPrices
{
    /// <summary>
    /// Represents tick price.
    /// </summary>
    [PublicAPI]
    public class TickPriceModel
    {
        /// <summary>
        /// The name of tick price source.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// The name of asset pair.
        /// </summary>
        public string Asset { get; set; }

        /// <summary>
        /// The timestamp of tick prices.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The ask price.
        /// </summary>
        public decimal Ask { get; set; }

        /// <summary>
        /// The bid price.
        /// </summary>
        public decimal Bid { get; set; }
    }
}
