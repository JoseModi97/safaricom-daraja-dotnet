using System;
using System.Threading;
using System.Threading.Tasks;
using Safaricom.Daraja.Models;
using Safaricom.Daraja.Transport;

namespace Safaricom.Daraja.Services
{
    /// <summary>Queries the status of a previous M-Pesa transaction.</summary>
    public class TransactionStatusClient
    {
        private readonly DarajaTransport _transport;

        public TransactionStatusClient(DarajaTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        public Task<DarajaAckResponse> QueryAsync(TransactionStatusRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _transport.PostAsync<DarajaAckResponse>("/mpesa/transactionstatus/v1/query", request, cancellationToken);
        }
    }
}
