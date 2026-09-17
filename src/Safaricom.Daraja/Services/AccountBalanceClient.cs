using System;
using System.Threading;
using System.Threading.Tasks;
using Safaricom.Daraja.Models;
using Safaricom.Daraja.Transport;

namespace Safaricom.Daraja.Services
{
    /// <summary>Queries the account balance of a shortcode.</summary>
    public class AccountBalanceClient
    {
        private readonly DarajaTransport _transport;

        public AccountBalanceClient(DarajaTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        public Task<DarajaAckResponse> QueryAsync(AccountBalanceRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _transport.PostAsync<DarajaAckResponse>("/mpesa/accountbalance/v1/query", request, cancellationToken);
        }
    }
}
