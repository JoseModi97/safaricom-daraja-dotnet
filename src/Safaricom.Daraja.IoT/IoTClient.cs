using System;
using System.Net.Http;
using Safaricom.Daraja.IoT.Services;
using Safaricom.Daraja.IoT.Transport;

namespace Safaricom.Daraja.IoT
{
    /// <summary>
    /// Master client for Safaricom's IoT SIM-portal and IMSI APIs. These are a distinct product
    /// from core Daraja — separate auth/header scheme, no public documentation — bundled here
    /// because they shipped in the same source collection this SDK was built from.
    /// </summary>
    public class IoTClient
    {
        private readonly IoTTransport _transport;

        public IoTConfig Config => _transport.Config;

        public SimMessagingClient Messaging { get; }
        public SimOperationsClient SimOperations { get; }
        public ImsiClient Imsi { get; }

        public IoTClient(IoTConfig config, HttpClient? httpClient = null)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            _transport = new IoTTransport(config, httpClient);

            Messaging = new SimMessagingClient(_transport);
            SimOperations = new SimOperationsClient(_transport);
            Imsi = new ImsiClient(_transport);
        }
    }
}
