using JetBrains.Annotations;
using Lykke.Sdk.Settings;
using Lykke.Service.Balances.Client;
using Lykke.Service.PlaceOrderBook.Settings.Clients;
using Lykke.Service.PlaceOrderBook.Settings.ServiceSettings;

namespace Lykke.Service.PlaceOrderBook.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public PlaceOrderBookSettings PlaceOrderBookService { get; set; }

        public BalancesServiceClientSettings BalancesServiceClient { get; set; }

        public RateCalculatorServiceClient RateCalculatorServiceClient { get; set; }
    }
}
