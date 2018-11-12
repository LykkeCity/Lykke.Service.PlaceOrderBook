using System.Threading.Tasks;

namespace Lykke.Service.PlaceOrderBook.Core.Messaging
{
    public interface IIndexTickPriceBatchPublisher
    {
        Task Publish(IndexTickPriceBatch indexTickPriceBatch);
    }
}
