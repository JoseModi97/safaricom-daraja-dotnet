using System;
using System.Threading;
using System.Threading.Tasks;
using Safaricom.Daraja.Models;
using Safaricom.Daraja.Transport;

namespace Safaricom.Daraja.Services
{
    /// <summary>M-Pesa Ratiba: customer-authorized recurring standing order payments.</summary>
    public class RatibaClient
    {
        private readonly DarajaTransport _transport;

        public RatibaClient(DarajaTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        public Task<RatibaCreateResponse> CreateAsync(RatibaCreateRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _transport.PostAsync<RatibaCreateResponse>("/standingorder/v1/createStandingOrderExternal", request, cancellationToken);
        }
    }
}
