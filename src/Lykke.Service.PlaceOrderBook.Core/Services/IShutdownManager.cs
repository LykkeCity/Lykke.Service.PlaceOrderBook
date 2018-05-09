using System.Threading.Tasks;

namespace Lykke.Service.PlaceOrderBook.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}
