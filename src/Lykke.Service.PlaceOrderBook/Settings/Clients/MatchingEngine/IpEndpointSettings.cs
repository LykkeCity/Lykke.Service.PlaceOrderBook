using JetBrains.Annotations;

namespace Lykke.Service.PlaceOrderBook.Settings.Clients.MatchingEngine
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class IpEndpointSettings
    {
        public string Host { get; set; }

        public int Port { get; set; }
    }
}
