using System;
using System.Threading;
using System.Threading.Tasks;
using Safaricom.Daraja.Models;
using Safaricom.Daraja.Transport;

namespace Safaricom.Daraja.Services
{
    /// <summary>Business-to-Business: moves funds between shortcodes, and remits taxes to KRA.</summary>
    public class B2BClient
    {
        private readonly DarajaTransport _transport;

        public B2BClient(DarajaTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        public Task<DarajaAckResponse> SendAsync(B2BRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _transport.PostAsync<DarajaAckResponse>("/mpesa/b2b/v1/paymentrequest", request, cancellationToken);
        }

        /// <summary>
        /// Convenience wrapper for Tax Remittance: this is the same B2B endpoint with
        /// <see cref="B2BCommandId.PayTaxes"/> and <paramref name="paymentRegistrationNumber"/>
        /// (the KRA PRN) as the AccountReference.
        /// </summary>
        /// <param name="initiator">The API operator username authorized for B2B on your Daraja app.</param>
        /// <param name="securityCredential">Build with <see cref="Security.SecurityCredentialEncryptor"/>.</param>
        /// <param name="businessShortCode">Your paying business shortcode.</param>
        /// <param name="kraShortCode">
        /// KRA's receiving Daraja shortcode for tax remittance, as shown on your Daraja portal
        /// app credentials page. This SDK does not hardcode it since Safaricom documents it as
        /// an app-specific value rather than a universal constant.
        /// </param>
        /// <param name="paymentRegistrationNumber">The KRA Payment Registration Number (PRN) being settled.</param>
        /// <param name="amount">The amount to remit.</param>
        /// <param name="remarks">Free-text remarks describing the remittance.</param>
        /// <param name="queueTimeOutUrl">Called if Daraja cannot process the request within its internal timeout.</param>
        /// <param name="resultUrl">Called with the final outcome once Daraja processes the request.</param>
        /// <param name="cancellationToken"></param>
        public Task<DarajaAckResponse> PayTaxesAsync(
            string initiator,
            string securityCredential,
            string businessShortCode,
            string kraShortCode,
            string paymentRegistrationNumber,
            decimal amount,
            string remarks,
            string queueTimeOutUrl,
            string resultUrl,
            CancellationToken cancellationToken = default)
        {
            var request = new B2BRequest
            {
                Initiator = initiator,
                SecurityCredential = securityCredential,
                CommandID = B2BCommandId.PayTaxes.ToString(),
                SenderIdentifierType = ((int)DarajaIdentifierType.Shortcode).ToString(),
                RecieverIdentifierType = ((int)DarajaIdentifierType.Shortcode).ToString(),
                Amount = amount.ToString("0"),
                PartyA = businessShortCode,
                PartyB = kraShortCode,
                AccountReference = paymentRegistrationNumber,
                Remarks = remarks,
                QueueTimeOutURL = queueTimeOutUrl,
                ResultURL = resultUrl
            };

            return SendAsync(request, cancellationToken);
        }
    }
}
