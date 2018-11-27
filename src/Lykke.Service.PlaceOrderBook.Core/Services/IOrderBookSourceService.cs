using Lykke.Service.PlaceOrderBook.Core.Settings;

namespace Lykke.Service.PlaceOrderBook.Core.Services
{
    public interface IOrderBookSourceService
    {
        void Configure(OrderBookSourceConfiguration configuration);
    }
}
