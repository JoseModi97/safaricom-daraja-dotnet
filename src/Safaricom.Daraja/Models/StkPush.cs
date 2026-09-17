using System.Text.Json.Serialization;

namespace Safaricom.Daraja.Models
{
    /// <summary>Whether the STK prompt is for a Paybill or a Buy Goods (till) transaction.</summary>
    public enum StkTransactionType
    {
        CustomerPayBillOnline,
        CustomerBuyGoodsOnline
    }

    /// <summary>
    /// Input for initiating an M-Pesa Express (STK Push) request. <see cref="Services.StkPushClient"/>
    /// computes <c>Password</c> and <c>Timestamp</c> for you from <see cref="ShortCode"/> and <see cref="Passkey"/>.
    /// </summary>
    public class StkPushRequest
    {
        /// <summary>The Paybill or Till shortcode the prompt is raised against.</summary>
        public string ShortCode { get; set; } = string.Empty;

        /// <summary>The Lipa na M-Pesa Online passkey for <see cref="ShortCode"/>, from the Daraja portal.</summary>
        public string Passkey { get; set; } = string.Empty;

        public StkTransactionType TransactionType { get; set; } = StkTransactionType.CustomerPayBillOnline;

        public decimal Amount { get; set; }

        /// <summary>The paying customer's MSISDN, e.g. 254712345678. Also used as PartyA unless overridden.</summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>Defaults to <see cref="PhoneNumber"/> if not set.</summary>
        public string? PartyA { get; set; }

        /// <summary>Defaults to <see cref="ShortCode"/> if not set.</summary>
        public string? PartyB { get; set; }

        public string CallBackURL { get; set; } = string.Empty;

        public string AccountReference { get; set; } = string.Empty;

        public string TransactionDesc { get; set; } = string.Empty;
    }

    internal class StkPushWireRequest
    {
        [JsonPropertyName("BusinessShortCode")]
        public string BusinessShortCode { get; set; } = string.Empty;

        [JsonPropertyName("Password")]
        public string Password { get; set; } = string.Empty;

        [JsonPropertyName("Timestamp")]
        public string Timestamp { get; set; } = string.Empty;

        [JsonPropertyName("TransactionType")]
        public string TransactionType { get; set; } = string.Empty;

        [JsonPropertyName("Amount")]
        public string Amount { get; set; } = string.Empty;

        [JsonPropertyName("PartyA")]
        public string PartyA { get; set; } = string.Empty;

        [JsonPropertyName("PartyB")]
        public string PartyB { get; set; } = string.Empty;

        [JsonPropertyName("PhoneNumber")]
        public string PhoneNumber { get; set; } = string.Empty;

        [JsonPropertyName("CallBackURL")]
        public string CallBackURL { get; set; } = string.Empty;

        [JsonPropertyName("AccountReference")]
        public string AccountReference { get; set; } = string.Empty;

        [JsonPropertyName("TransactionDesc")]
        public string TransactionDesc { get; set; } = string.Empty;
    }

    public class StkPushResponse
    {
        [JsonPropertyName("MerchantRequestID")]
        public string? MerchantRequestId { get; set; }

        [JsonPropertyName("CheckoutRequestID")]
        public string? CheckoutRequestId { get; set; }

        [JsonPropertyName("ResponseCode")]
        public string? ResponseCode { get; set; }

        [JsonPropertyName("ResponseDescription")]
        public string? ResponseDescription { get; set; }

        [JsonPropertyName("CustomerMessage")]
        public string? CustomerMessage { get; set; }

        [JsonIgnore]
        public bool IsAccepted => ResponseCode == "0";
    }

    public class StkPushQueryRequest
    {
        public string ShortCode { get; set; } = string.Empty;
        public string Passkey { get; set; } = string.Empty;
        public string CheckoutRequestId { get; set; } = string.Empty;
    }

    internal class StkPushQueryWireRequest
    {
        [JsonPropertyName("BusinessShortCode")]
        public string BusinessShortCode { get; set; } = string.Empty;

        [JsonPropertyName("Password")]
        public string Password { get; set; } = string.Empty;

        [JsonPropertyName("Timestamp")]
        public string Timestamp { get; set; } = string.Empty;

        [JsonPropertyName("CheckoutRequestID")]
        public string CheckoutRequestID { get; set; } = string.Empty;
    }

    public class StkPushQueryResponse
    {
        [JsonPropertyName("ResponseCode")]
        public string? ResponseCode { get; set; }

        [JsonPropertyName("ResponseDescription")]
        public string? ResponseDescription { get; set; }

        [JsonPropertyName("MerchantRequestID")]
        public string? MerchantRequestId { get; set; }

        [JsonPropertyName("CheckoutRequestID")]
        public string? CheckoutRequestId { get; set; }

        /// <summary>0 = success, 1032 = cancelled by user, 1037 = timeout, etc. Only present once the transaction has settled.</summary>
        [JsonPropertyName("ResultCode")]
        public string? ResultCode { get; set; }

        [JsonPropertyName("ResultDesc")]
        public string? ResultDesc { get; set; }
    }

    /// <summary>The envelope Daraja posts to your STK Push <c>CallBackURL</c>: <c>{ "Body": { "stkCallback": { ... } } }</c>.</summary>
    public class StkCallbackEnvelope
    {
        [JsonPropertyName("Body")]
        public StkCallbackBody? Body { get; set; }
    }

    public class StkCallbackBody
    {
        [JsonPropertyName("stkCallback")]
        public StkCallback? StkCallback { get; set; }
    }

    public class StkCallback
    {
        [JsonPropertyName("MerchantRequestID")]
        public string? MerchantRequestId { get; set; }

        [JsonPropertyName("CheckoutRequestID")]
        public string? CheckoutRequestId { get; set; }

        /// <summary>0 = success. Any other value means the customer cancelled, timed out, or the request otherwise failed.</summary>
        [JsonPropertyName("ResultCode")]
        public int ResultCode { get; set; }

        [JsonPropertyName("ResultDesc")]
        public string? ResultDesc { get; set; }

        /// <summary>Only present when <see cref="ResultCode"/> is 0.</summary>
        [JsonPropertyName("CallbackMetadata")]
        public StkCallbackMetadata? CallbackMetadata { get; set; }

        [JsonIgnore]
        public bool IsSuccess => ResultCode == 0;

        /// <summary>Looks up a named entry from <see cref="CallbackMetadata"/> (e.g. "Amount", "MpesaReceiptNumber", "TransactionDate", "PhoneNumber").</summary>
        public object? GetMetadataValue(string name)
        {
            return CallbackMetadata?.Item?.Find(i => string.Equals(i.Name, name, System.StringComparison.OrdinalIgnoreCase))?.Value;
        }
    }

    public class StkCallbackMetadata
    {
        [JsonPropertyName("Item")]
        public System.Collections.Generic.List<StkCallbackMetadataItem>? Item { get; set; }
    }

    public class StkCallbackMetadataItem
    {
        [JsonPropertyName("Name")]
        public string? Name { get; set; }

        [JsonPropertyName("Value")]
        public object? Value { get; set; }
    }
}
