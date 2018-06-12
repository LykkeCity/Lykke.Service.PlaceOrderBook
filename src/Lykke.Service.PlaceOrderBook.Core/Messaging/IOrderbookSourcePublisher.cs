using System.Threading.Tasks;
using Lykke.Common.ExchangeAdapter.Contracts;

namespace Lykke.Service.PlaceOrderBook.Core.Messaging
{
    public interface IOrderbookSourcePublisher
    { 
        Task Publish(OrderBook orderbok);
    }
}
