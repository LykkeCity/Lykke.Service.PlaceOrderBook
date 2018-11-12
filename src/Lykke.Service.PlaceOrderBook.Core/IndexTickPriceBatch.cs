using System.Collections.Generic;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Service.CryptoIndex.Contract;

namespace Lykke.Service.PlaceOrderBook.Core
{
    public class IndexTickPriceBatch
    {
        public IReadOnlyCollection<TickPrice> TickPrices { get; set; } 

        public IReadOnlyCollection<IndexTickPrice> IndexTickPrices { get; set; }
    }
}
