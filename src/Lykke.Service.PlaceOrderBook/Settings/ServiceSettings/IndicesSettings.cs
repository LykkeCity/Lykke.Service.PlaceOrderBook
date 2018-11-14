using Lykke.Service.PlaceOrderBook.Settings.Rabbit;

namespace Lykke.Service.PlaceOrderBook.Settings.ServiceSettings
{
    public class IndicesSettings
    {
        public PublisherSettings TickPriceExchange { get; set; }

        public PublisherSettings IndexTickPriceExchange { get; set; }
    }
}
