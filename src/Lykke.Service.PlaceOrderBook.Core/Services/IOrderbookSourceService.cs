using Lykke.Service.PlaceOrderBook.Core.Settings;

namespace Lykke.Service.PlaceOrderBook.Core.Services
{
    public interface IOrderbookSourceService
    {
        void Configure(OrderbookSourceConfiguration configuration);
    }
}
