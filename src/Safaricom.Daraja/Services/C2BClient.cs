using System;
using System.Threading;
using System.Threading.Tasks;
using Safaricom.Daraja.Models;
using Safaricom.Daraja.Transport;

namespace Safaricom.Daraja.Services
{
    /// <summary>Customer-to-Business: register callback URLs, and (Sandbox only) simulate incoming payments.</summary>
    public class C2BClient
    {
        private readonly DarajaTransport _transport;

        public C2BClient(DarajaTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        /// <summary>Registers the Validation and Confirmation callback URLs for a shortcode.</summary>
        public Task<C2BRegisterUrlResponse> RegisterUrlAsync(C2BRegisterUrlRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _transport.PostAsync<C2BRegisterUrlResponse>("/mpesa/c2b/v1/registerurl", request, cancellationToken);
        }

        /// <summary>
        /// Simulates an incoming C2B payment. Sandbox only — Daraja Production has no simulate
        /// endpoint, since live customer payments arrive by definition through real M-Pesa traffic.
        /// </summary>
        public Task<C2BSimulateResponse> SimulateAsync(C2BSimulateRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (_transport.Config.Environment == DarajaEnvironment.Production)
            {
                throw new InvalidOperationException("C2B Simulate is only available in the Sandbox environment.");
            }

            return _transport.PostAsync<C2BSimulateResponse>("/mpesa/c2b/v1/simulate", request, cancellationToken);
        }
    }
}
