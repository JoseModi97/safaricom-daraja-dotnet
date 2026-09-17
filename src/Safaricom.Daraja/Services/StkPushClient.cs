using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Safaricom.Daraja.Models;
using Safaricom.Daraja.Transport;

namespace Safaricom.Daraja.Services
{
    /// <summary>M-Pesa Express (STK Push): prompts a customer's phone for a PIN, and lets you poll the outcome.</summary>
    public class StkPushClient
    {
        private readonly DarajaTransport _transport;

        public StkPushClient(DarajaTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        /// <summary>Raises an STK push prompt on the customer's phone.</summary>
        public Task<StkPushResponse> PushAsync(StkPushRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var password = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{request.ShortCode}{request.Passkey}{timestamp}"));

            var wireRequest = new StkPushWireRequest
            {
                BusinessShortCode = request.ShortCode,
                Password = password,
                Timestamp = timestamp,
                TransactionType = request.TransactionType.ToString(),
                Amount = request.Amount.ToString("0"),
                PartyA = string.IsNullOrWhiteSpace(request.PartyA) ? request.PhoneNumber : request.PartyA!,
                PartyB = string.IsNullOrWhiteSpace(request.PartyB) ? request.ShortCode : request.PartyB!,
                PhoneNumber = request.PhoneNumber,
                CallBackURL = request.CallBackURL,
                AccountReference = request.AccountReference,
                TransactionDesc = request.TransactionDesc
            };

            return _transport.PostAsync<StkPushResponse>("/mpesa/stkpush/v1/processrequest", wireRequest, cancellationToken);
        }

        /// <summary>Queries the outcome of a previously raised STK push.</summary>
        public Task<StkPushQueryResponse> QueryAsync(StkPushQueryRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var password = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{request.ShortCode}{request.Passkey}{timestamp}"));

            var wireRequest = new StkPushQueryWireRequest
            {
                BusinessShortCode = request.ShortCode,
                Password = password,
                Timestamp = timestamp,
                CheckoutRequestID = request.CheckoutRequestId
            };

            return _transport.PostAsync<StkPushQueryResponse>("/mpesa/stkpushquery/v1/query", wireRequest, cancellationToken);
        }
    }
}
