namespace Lykke.Service.PlaceOrderBook.Core.Settings
{
    public class OrderBookSourceConfiguration
    {
        public OrderBookSourceConfiguration(string source, string assetPairId, int count, double priceDelta)
        {
            Source = source;
            AssetPairId = assetPairId;
            Count = count;
            PriceDelta = priceDelta;
        }

        public string Source { get; }

        public string AssetPairId { get; }

        public int Count { get; }

        public double PriceDelta { get; }
    }
}
