using System;
using System.Threading;
using System.Threading.Tasks;
using Safaricom.Daraja.Models;
using Safaricom.Daraja.Transport;

namespace Safaricom.Daraja.Services
{
    /// <summary>Lipa na Bonga: redeem a customer's Bonga points against a payment, or price points into currency.</summary>
    public class LipaNaBongaClient
    {
        private readonly DarajaTransport _transport;

        public LipaNaBongaClient(DarajaTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        public async Task<DarajaRawResult> RedeemPaybillAsync(BongaRedeemPaybillRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var json = await _transport.PostAsync<string>("/v1/lipa/na/bonga/redeem-paybill", request, cancellationToken).ConfigureAwait(false);
            return new DarajaRawResult { Json = json };
        }

        public async Task<DarajaRawResult> CalculatePointsAsync(BongaCalculatePointsRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var json = await _transport.PostAsync<string>("/v1/lipa/na/bonga/calculator-points", request, cancellationToken).ConfigureAwait(false);
            return new DarajaRawResult { Json = json };
        }
    }
}
