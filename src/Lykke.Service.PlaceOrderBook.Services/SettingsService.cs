using System.Collections.Generic;

namespace Lykke.Service.PlaceOrderBook.Services
{
    public class SettingsService
    {
        public SettingsService(
            IReadOnlyCollection<string> trustedClients,
            IReadOnlyCollection<string> balanceAssets)
        {
            TrustedClients = trustedClients;
            BalanceAssets = balanceAssets;
        }
        
        public IReadOnlyCollection<string> TrustedClients { get; }
        
        public IReadOnlyCollection<string> BalanceAssets { get; }
    }
}
