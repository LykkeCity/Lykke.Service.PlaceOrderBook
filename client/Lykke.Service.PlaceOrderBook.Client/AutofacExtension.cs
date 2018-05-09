using System;
using Autofac;
using Common.Log;

namespace Lykke.Service.PlaceOrderBook.Client
{
    public static class AutofacExtension
    {
        public static void RegisterPlaceOrderBookClient(this ContainerBuilder builder, string serviceUrl, ILog log)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (serviceUrl == null) throw new ArgumentNullException(nameof(serviceUrl));
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (string.IsNullOrWhiteSpace(serviceUrl))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(serviceUrl));

            builder.RegisterType<PlaceOrderBookClient>()
                .WithParameter("serviceUrl", serviceUrl)
                .As<IPlaceOrderBookClient>()
                .SingleInstance();
        }

        public static void RegisterPlaceOrderBookClient(this ContainerBuilder builder, PlaceOrderBookServiceClientSettings settings, ILog log)
        {
            builder.RegisterPlaceOrderBookClient(settings?.ServiceUrl, log);
        }
    }
}
