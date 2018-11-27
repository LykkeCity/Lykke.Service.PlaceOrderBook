using System.Text;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Lykke.Service.PlaceOrderBook.RabbitMq
{
    public sealed class GenericRabbitModelConverter<T> : IRabbitMqSerializer<T>, IMessageDeserializer<T>
    {
        private const string Iso8601DateFormat = @"yyyy-MM-ddTHH:mm:ss.fffzzz";

        private readonly JsonSerializerSettings _serializeSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            DateFormatString = Iso8601DateFormat
        };

        private readonly JsonSerializerSettings _deserializeSettings = new JsonSerializerSettings
        {
            DateFormatString = Iso8601DateFormat
        };

        public byte[] Serialize(T model)
        {
            string json = JsonConvert.SerializeObject(model, _serializeSettings);

            return Encoding.UTF8.GetBytes(json);
        }

        public T Deserialize(byte[] data)
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data), _deserializeSettings);
        }
    }
}
