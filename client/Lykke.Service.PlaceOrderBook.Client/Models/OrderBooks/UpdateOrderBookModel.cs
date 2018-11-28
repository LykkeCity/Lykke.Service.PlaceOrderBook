using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Service.PlaceOrderBook.Client.Models.OrderBooks
{
    [PublicAPI]
    public class UpdateOrderBookModel
    {
        public string ClientId { get; set; }
        
        public string AssetPair { get; set; }
        
        public List<LimitOrderModel> Orders { get; set; }
    }
}
