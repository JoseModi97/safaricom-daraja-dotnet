using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Safaricom.Daraja.Models;
using Safaricom.Daraja.Transport;

namespace Safaricom.Daraja.Services
{
    /// <summary>Bill Manager: opt in, then raise and cancel invoices. Payment reconciliation arrives as a webhook to your opt-in callback URL.</summary>
    public class BillManagerClient
    {
        private readonly DarajaTransport _transport;

        public BillManagerClient(DarajaTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        public Task<BillManagerResponse> OptInAsync(BillManagerOptInRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _transport.PostAsync<BillManagerResponse>("/v1/billmanager-invoice/optin", request, cancellationToken);
        }

        public Task<BillManagerResponse> SendSingleInvoiceAsync(BillManagerInvoice invoice, CancellationToken cancellationToken = default)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));
            return _transport.PostAsync<BillManagerResponse>("/v1/billmanager-invoice/single-invoicing", invoice, cancellationToken);
        }

        public Task<BillManagerResponse> SendBulkInvoicesAsync(IReadOnlyList<BillManagerInvoice> invoices, CancellationToken cancellationToken = default)
        {
            if (invoices == null || invoices.Count == 0) throw new ArgumentException("At least one invoice is required.", nameof(invoices));
            return _transport.PostAsync<BillManagerResponse>("/v1/billmanager-invoice/bulk-invoicing", invoices, cancellationToken);
        }

        public Task<BillManagerResponse> CancelSingleInvoiceAsync(BillManagerCancelSingleInvoiceRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _transport.PostAsync<BillManagerResponse>("/v1/billmanager-invoice/cancel-single-invoice", request, cancellationToken);
        }

        public Task<BillManagerResponse> CancelBulkInvoicesAsync(BillManagerCancelBulkInvoicesRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _transport.PostAsync<BillManagerResponse>("/v1/billmanager-invoice/cancel-bulk-invoices", request, cancellationToken);
        }
    }
}
