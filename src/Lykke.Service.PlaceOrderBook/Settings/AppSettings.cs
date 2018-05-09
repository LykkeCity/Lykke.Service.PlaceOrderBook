using JetBrains.Annotations;
using Lykke.Service.PlaceOrderBook.Settings.ServiceSettings;
using Lykke.Service.PlaceOrderBook.Settings.SlackNotifications;

namespace Lykke.Service.PlaceOrderBook.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings
    {
        public PlaceOrderBookSettings PlaceOrderBookService { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }
}
