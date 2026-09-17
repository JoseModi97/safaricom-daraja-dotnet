using System.Text.Json.Serialization;

namespace Safaricom.Daraja.Models
{
    public enum C2BResponseType
    {
        Completed,
        Cancelled
    }

    public enum C2BCommandId
    {
        CustomerPayBillOnline,
        CustomerBuyGoodsOnline
    }

    /// <summary>Registers the Validation and Confirmation callback URLs for a C2B shortcode.</summary>
    public class C2BRegisterUrlRequest
    {
        [JsonPropertyName("ShortCode")]
        public string ShortCode { get; set; } = string.Empty;

        [JsonPropertyName("ResponseType")]
        public string ResponseType { get; set; } = C2BResponseType.Completed.ToString();

        [JsonPropertyName("ConfirmationURL")]
        public string ConfirmationURL { get; set; } = string.Empty;

        [JsonPropertyName("ValidationURL")]
        public string ValidationURL { get; set; } = string.Empty;
    }

    public class C2BRegisterUrlResponse
    {
        /// <summary>
        /// Daraja's C2B register-url response historically ships this field misspelled as
        /// "OriginatorCoversationID" on the wire; this property maps to that exact spelling.
        /// </summary>
        [JsonPropertyName("OriginatorCoversationID")]
        public string? OriginatorConversationId { get; set; }

        [JsonPropertyName("ResponseCode")]
        public string? ResponseCode { get; set; }

        [JsonPropertyName("ResponseDescription")]
        public string? ResponseDescription { get; set; }
    }

    /// <summary>Simulates an incoming customer C2B payment. Sandbox only — Production has no simulate endpoint.</summary>
    public class C2BSimulateRequest
    {
        [JsonPropertyName("ShortCode")]
        public string ShortCode { get; set; } = string.Empty;

        [JsonPropertyName("CommandID")]
        public string CommandID { get; set; } = C2BCommandId.CustomerPayBillOnline.ToString();

        [JsonPropertyName("Amount")]
        public string Amount { get; set; } = string.Empty;

        [JsonPropertyName("Msisdn")]
        public string Msisdn { get; set; } = string.Empty;

        [JsonPropertyName("BillRefNumber")]
        public string BillRefNumber { get; set; } = string.Empty;
    }

    public class C2BSimulateResponse
    {
        [JsonPropertyName("OriginatorConversationID")]
        public string? OriginatorConversationId { get; set; }

        [JsonPropertyName("ConversationID")]
        public string? ConversationId { get; set; }

        [JsonPropertyName("ResponseDescription")]
        public string? ResponseDescription { get; set; }
    }

    /// <summary>
    /// The payload Daraja posts to your Validation/Confirmation URLs for an incoming C2B payment.
    /// </summary>
    public class C2BCallback
    {
        [JsonPropertyName("TransactionType")]
        public string? TransactionType { get; set; }

        [JsonPropertyName("TransID")]
        public string? TransId { get; set; }

        [JsonPropertyName("TransTime")]
        public string? TransTime { get; set; }

        [JsonPropertyName("TransAmount")]
        public string? TransAmount { get; set; }

        [JsonPropertyName("BusinessShortCode")]
        public string? BusinessShortCode { get; set; }

        [JsonPropertyName("BillRefNumber")]
        public string? BillRefNumber { get; set; }

        [JsonPropertyName("InvoiceNumber")]
        public string? InvoiceNumber { get; set; }

        [JsonPropertyName("OrgAccountBalance")]
        public string? OrgAccountBalance { get; set; }

        [JsonPropertyName("ThirdPartyTransID")]
        public string? ThirdPartyTransId { get; set; }

        [JsonPropertyName("MSISDN")]
        public string? Msisdn { get; set; }

        [JsonPropertyName("FirstName")]
        public string? FirstName { get; set; }

        [JsonPropertyName("MiddleName")]
        public string? MiddleName { get; set; }

        [JsonPropertyName("LastName")]
        public string? LastName { get; set; }
    }

    /// <summary>The response your Validation endpoint must return to accept or reject the payment.</summary>
    public class C2BValidationResponse
    {
        [JsonPropertyName("ResultCode")]
        public string ResultCode { get; set; } = "0";

        [JsonPropertyName("ResultDesc")]
        public string ResultDesc { get; set; } = "Accepted";

        public static C2BValidationResponse Accept(string description = "Accepted") => new C2BValidationResponse { ResultCode = "0", ResultDesc = description };

        public static C2BValidationResponse Reject(string description, string resultCode = "C2B00016") => new C2BValidationResponse { ResultCode = resultCode, ResultDesc = description };
    }
}
