using System;
using System.Threading;
using System.Threading.Tasks;
using Safaricom.Daraja.Models;
using Safaricom.Daraja.Transport;

namespace Safaricom.Daraja.Services
{
    /// <summary>Reverses a previously completed M-Pesa transaction.</summary>
    public class ReversalClient
    {
        private readonly DarajaTransport _transport;

        public ReversalClient(DarajaTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        public Task<DarajaAckResponse> SendAsync(ReversalRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _transport.PostAsync<DarajaAckResponse>("/mpesa/reversal/v1/request", request, cancellationToken);
        }
    }
}
