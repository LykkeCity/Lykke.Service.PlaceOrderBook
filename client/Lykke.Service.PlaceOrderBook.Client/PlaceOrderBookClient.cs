﻿using System;
using Common.Log;

namespace Lykke.Service.PlaceOrderBook.Client
{
    public class PlaceOrderBookClient : IPlaceOrderBookClient, IDisposable
    {
        private readonly ILog _log;

        public PlaceOrderBookClient(string serviceUrl, ILog log)
        {
            _log = log;
        }

        public void Dispose()
        {
            //if (_service == null)
            //    return;
            //_service.Dispose();
            //_service = null;
        }
    }
}
