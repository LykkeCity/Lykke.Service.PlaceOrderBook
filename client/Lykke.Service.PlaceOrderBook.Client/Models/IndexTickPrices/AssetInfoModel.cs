using JetBrains.Annotations;

namespace Lykke.Service.PlaceOrderBook.Client.Models.IndexTickPrices
{
    /// <summary>
    /// Asset information.
    /// </summary>
    [PublicAPI]
    public class AssetInfoModel
    {
        /// <summary>
        /// Identifier of the asset.
        /// </summary>
        public string AssetId { get; }

        /// <summary>
        /// Weight of the asset.
        /// </summary>
        public decimal Weight { get; }

        /// <summary>
        /// Middle price of the asset.
        /// </summary>
        public decimal Price { get; }

        /// <summary>
        /// True if the asset was 'frozen'.
        /// </summary>
        public bool IsDisabled { get; }
    }
}
