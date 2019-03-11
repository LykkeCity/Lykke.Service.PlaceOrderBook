using System;
using Lykke.AzureStorage.Tables;

namespace Lykke.Service.PlaceOrderBook.AzureRepositories.Orders
{
    public class OrderEntity : AzureTableEntity
    {
        public string ClientId
        {
            get => PartitionKey;
            set => PartitionKey = value;
        }

        public string OrderId
        {
            get => RowKey;
            set => RowKey = value;
        }
        
        public string Instrument { get; set; }
        
        public decimal Price { get; set; }
        
        public decimal OriginalAmount { get; set; }
        
        public string TradeType { get; set; }
        
        public DateTime CreatedTime { get; set; }
        
        public decimal AvgExecutionPrice { get; set; }

        public int CountExecute { get; set; }
        
        public decimal SumExecutePrice { get; set; }

        public string Status { get; set; }
        
        public decimal ExecutedAmount { get; set; }
        
        public decimal RemainingAmount { get; set; }
    }
}
