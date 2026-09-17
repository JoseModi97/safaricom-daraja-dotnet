using System;
using System.Threading;
using System.Threading.Tasks;
using Safaricom.Daraja.Models;
using Safaricom.Daraja.Transport;

namespace Safaricom.Daraja.Services
{
    /// <summary>Pull Transactions API: register a shortcode, then pull its settled transactions on demand.</summary>
    public class PullApiClient
    {
        private readonly DarajaTransport _transport;

        public PullApiClient(DarajaTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        public Task<PullApiRegisterResponse> RegisterAsync(PullApiRegisterRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _transport.PostAsync<PullApiRegisterResponse>("/pulltransactions/v1/register", request, cancellationToken);
        }

        public Task<PullApiQueryResponse> QueryAsync(PullApiQueryRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _transport.PostAsync<PullApiQueryResponse>("/pulltransactions/v1/query", request, cancellationToken);
        }
    }
}
