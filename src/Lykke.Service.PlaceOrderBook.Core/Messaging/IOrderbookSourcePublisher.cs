using System.Threading.Tasks;
using Lykke.MatchingEngine.ExchangeModels;

namespace Lykke.Service.PlaceOrderBook.Core.Messaging
{
    public interface IOrderbookSourcePublisher
    { 
        Task Publish(OrderBook orderbok);
    }
}
