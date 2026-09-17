using System;
using System.Threading;
using System.Threading.Tasks;
using Safaricom.Daraja.Models;
using Safaricom.Daraja.Transport;

namespace Safaricom.Daraja.Services
{
    /// <summary>Business-to-Customer: pays out from a business shortcode to a customer MSISDN (salary, business, or promotion payments, including B2Pochi).</summary>
    public class B2CClient
    {
        private readonly DarajaTransport _transport;

        public B2CClient(DarajaTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        public Task<DarajaAckResponse> SendAsync(B2CRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _transport.PostAsync<DarajaAckResponse>("/mpesa/b2c/v1/paymentrequest", request, cancellationToken);
        }
    }
}
