using JetBrains.Annotations;

namespace Lykke.Service.PlaceOrderBook.Client.Models.OrderBooks
{
    [PublicAPI]
    public class OrderModel
    {
        public string TradeType { get; set; }

        public double Price { get; set; }

        public double Volume { get; set; }
    }
}
