using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.PlaceOrderBook.Settings.ServiceSettings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}
