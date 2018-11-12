using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.PlaceOrderBook.Settings.Rabbit
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class PublisherSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }

        public string Exchange { get; set; }

        public bool IsDurable { get; set; }
    }
}
