using System;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.AzureStorage.Tables;

namespace Lykke.Service.PlaceOrderBook.AzureRepositories.Orders
{
    public class OrderRepository
    {
        private readonly INoSQLTableStorage<OrderEntity> _tableStorage;

        public OrderRepository(INoSQLTableStorage<OrderEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task AddOrReplace(OrderEntity order)
        {
            await _tableStorage.InsertOrReplaceAsync(order);
        }

        public async Task<OrderEntity> GetOrder(string clientId, string orderId)
        {
            try
            {
                return await _tableStorage.GetDataAsync(clientId, orderId);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
