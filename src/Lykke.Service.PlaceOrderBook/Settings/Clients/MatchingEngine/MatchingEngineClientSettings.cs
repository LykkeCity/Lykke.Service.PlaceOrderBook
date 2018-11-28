using JetBrains.Annotations;

namespace Lykke.Service.PlaceOrderBook.Settings.Clients.MatchingEngine
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class MatchingEngineClientSettings
    {
        public IpEndpointSettings IpEndpoint { get; set; }
    }
}
