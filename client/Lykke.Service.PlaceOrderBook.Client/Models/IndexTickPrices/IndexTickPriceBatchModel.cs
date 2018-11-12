using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Service.PlaceOrderBook.Client.Models.IndexTickPrices
{
    /// <summary>
    /// Represents collection of tick prices and index tick prices.
    /// </summary>
    [PublicAPI]
    public class IndexTickPriceBatchModel
    {
        /// <summary>
        /// Collection of tick prices.
        /// </summary>
        public IReadOnlyCollection<TickPriceModel> TickPrices { get; set; }

        /// <summary>
        /// Collection of index tick prices.
        /// </summary>
        public IReadOnlyCollection<IndexTickPriceModel> IndexTickPrices { get; set; }
    }
}
