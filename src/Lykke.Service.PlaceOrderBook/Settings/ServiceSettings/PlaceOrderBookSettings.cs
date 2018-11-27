using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.Service.PlaceOrderBook.Settings.Clients.MatchingEngine;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.PlaceOrderBook.Settings.ServiceSettings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class PlaceOrderBookSettings
    {
        public DbSettings Db { get; set; }

        public MatchingEngineClientSettings MatchingEngine { get; set;  }

        public List<string> TrustedClientIds { get; set; }

        public List<string> BalanceAssets { get; set; }

        public ExchangeSettings LykkeTrade { get; set; }

        public OrderbookSourceSettings OrderbookSourceSettings { get; set; }

        public IndicesSettings Indices { get; set; }
    }

    public class ExchangeSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }

        public string Exchange { get; set; }

        public string QueueSuffix { get; set; }
    }

    public class OrderbookSourceSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }
        
        public string Exchange { get; set; }
    }
}
