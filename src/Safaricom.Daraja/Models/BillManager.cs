using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Safaricom.Daraja.Models
{
    /// <summary>
    /// Bill Manager's response envelope uses different field names ("rescode"/"resmsg")
    /// from the rest of Daraja's "ResponseCode"/"ResponseDescription" convention — a
    /// well-known inconsistency in this particular product.
    /// </summary>
    public class BillManagerResponse
    {
        [JsonPropertyName("rescode")]
        public string? ResultCode { get; set; }

        [JsonPropertyName("resmsg")]
        public string? ResultMessage { get; set; }

        [JsonIgnore]
        public bool IsSuccess => ResultCode == "200";
    }

    /// <summary>Opts a shortcode into Bill Manager, registering where reconciliation notifications are sent.</summary>
    public class BillManagerOptInRequest
    {
        [JsonPropertyName("shortcode")]
        public string ShortCode { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("officialContact")]
        public string OfficialContact { get; set; } = string.Empty;

        /// <summary>Whether Safaricom should send SMS payment reminders on your behalf. 1 = yes, 0 = no.</summary>
        [JsonPropertyName("sendReminders")]
        public int SendReminders { get; set; }

        /// <summary>Base64-encoded logo shown on invoices/reminders.</summary>
        [JsonPropertyName("logo")]
        public string? Logo { get; set; }

        [JsonPropertyName("callbackurl")]
        public string CallbackUrl { get; set; } = string.Empty;
    }

    public class BillManagerInvoiceItem
    {
        [JsonPropertyName("itemName")]
        public string ItemName { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
    }

    public class BillManagerInvoice
    {
        [JsonPropertyName("externalReference")]
        public string ExternalReference { get; set; } = string.Empty;

        [JsonPropertyName("billedFullName")]
        public string BilledFullName { get; set; } = string.Empty;

        [JsonPropertyName("billedPhoneNumber")]
        public string BilledPhoneNumber { get; set; } = string.Empty;

        /// <summary>yyyy-MM-dd.</summary>
        [JsonPropertyName("billedPeriod")]
        public string BilledPeriod { get; set; } = string.Empty;

        [JsonPropertyName("invoiceName")]
        public string InvoiceName { get; set; } = string.Empty;

        /// <summary>yyyy-MM-dd.</summary>
        [JsonPropertyName("dueDate")]
        public string DueDate { get; set; } = string.Empty;

        [JsonPropertyName("accountReference")]
        public string AccountReference { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("invoiceItems")]
        public List<BillManagerInvoiceItem>? InvoiceItems { get; set; }
    }

    public class BillManagerCancelSingleInvoiceRequest
    {
        [JsonPropertyName("externalReference")]
        public string ExternalReference { get; set; } = string.Empty;
    }

    public class BillManagerCancelBulkInvoicesRequest
    {
        [JsonPropertyName("externalReferences")]
        public List<string> ExternalReferences { get; set; } = new List<string>();
    }

    /// <summary>
    /// The reconciliation notification Safaricom posts to your opt-in callback URL whenever
    /// an invoice you raised through Bill Manager gets paid.
    /// </summary>
    public class BillManagerReconciliation
    {
        [JsonPropertyName("TransactionId")]
        public string? TransactionId { get; set; }

        [JsonPropertyName("PaidAmount")]
        public decimal PaidAmount { get; set; }

        [JsonPropertyName("CreditPartyName")]
        public string? CreditPartyName { get; set; }

        [JsonPropertyName("InvoiceName")]
        public string? InvoiceName { get; set; }

        [JsonPropertyName("ExternalReference")]
        public string? ExternalReference { get; set; }

        [JsonPropertyName("PaidAt")]
        public string? PaidAt { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string? PhoneNumber { get; set; }
    }
}
