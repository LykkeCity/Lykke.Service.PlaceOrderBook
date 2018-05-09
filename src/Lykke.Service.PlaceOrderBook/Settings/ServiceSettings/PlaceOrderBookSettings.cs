using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Service.PlaceOrderBook.Settings.ServiceSettings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class PlaceOrderBookSettings
    {
        public DbSettings Db { get; set; }

        public MatchingEngineSettings MatchingEngine { get; set;  }

        public List<string> TrustedClientIds { get; set; }
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
}
