using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.PlaceOrderBook.Settings.ServiceSettings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class PlaceOrderBookSettings
    {
        public DbSettings Db { get; set; }

        public MatchingEngineSettings MatchingEngine { get; set;  }

        public List<string> TrustedClientIds { get; set; }

        public List<string> BalanceAssets { get; set; }

        public ExchangeSettings LykkeTrade { get; set; }

        public OrderbookSourceSettings OrderbookSourceSettings { get; set; }

        public IndicesSettings Indices { get; set; }
    }

    public class MatchingEngineSettings
    {
        public IpEndpointSettings IpEndpoint { get; set; }
    }

    public class IpEndpointSettings
    {
        public string Host { get; set; }

        public int Port { get; set; }
    }

    public class ExchangeSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }

        public string Exchange { get; set; }

        public string QueueSuffix { get; set; }
    }

    public class BalancesServiceClient
    {
        [HttpCheck("api/isalive")]
        public string ServiceUrl { get; set; }
    }

    public class OrderbookSourceSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }
        public string Exchange { get; set; }
    }
}
