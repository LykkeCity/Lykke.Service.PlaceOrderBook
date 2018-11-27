using System.Threading.Tasks;
using Lykke.MatchingEngine.ExchangeModels;

namespace Lykke.Service.PlaceOrderBook.Core.Messaging
{
    public interface IOrderBookSourcePublisher
    { 
        Task PublishAsync(OrderBook orderBook);
    }
}
