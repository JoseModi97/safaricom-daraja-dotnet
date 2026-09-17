using System;
using System.Threading;
using System.Threading.Tasks;
using Safaricom.Daraja.Models;
using Safaricom.Daraja.Transport;

namespace Safaricom.Daraja.Services
{
    /// <summary>Generates scannable M-Pesa Dynamic QR codes.</summary>
    public class QrCodeClient
    {
        private readonly DarajaTransport _transport;

        public QrCodeClient(DarajaTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        public Task<QrCodeResponse> GenerateAsync(QrCodeRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var wireRequest = new QrCodeWireRequest
            {
                MerchantName = request.MerchantName,
                RefNo = request.ReferenceNo,
                Amount = (int)request.Amount,
                TrxCode = request.TransactionCode.ToWireValue(),
                CPI = request.CreditPartyIdentifier,
                Size = request.Size.ToString()
            };

            return _transport.PostAsync<QrCodeResponse>("/mpesa/qrcode/v1/generate", wireRequest, cancellationToken);
        }
    }
}
