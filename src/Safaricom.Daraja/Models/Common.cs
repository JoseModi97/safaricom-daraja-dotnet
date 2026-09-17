using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Safaricom.Daraja.Models
{
    /// <summary>
    /// Identifies the type of party (shortcode, till, or MSISDN) referenced by
    /// PartyA/PartyB/ReceiverParty on B2C, B2B, Reversal, and Account Balance calls.
    /// </summary>
    public enum DarajaIdentifierType
    {
        /// <summary>MSISDN (phone number). Wire value "1".</summary>
        Msisdn = 1,

        /// <summary>Till number. Wire value "2".</summary>
        TillNumber = 2,

        /// <summary>Shortcode. Wire value "4".</summary>
        Shortcode = 4
    }

    /// <summary>
    /// The immediate, synchronous acknowledgement Daraja returns for B2C, B2B, Reversal,
    /// Transaction Status, and Account Balance requests. The actual outcome arrives later,
    /// asynchronously, as a <see cref="DarajaResultCallback"/> posted to your ResultURL.
    /// </summary>
    public class DarajaAckResponse
    {
        [JsonPropertyName("OriginatorConversationID")]
        public string? OriginatorConversationId { get; set; }

        [JsonPropertyName("ConversationID")]
        public string? ConversationId { get; set; }

        [JsonPropertyName("ResponseCode")]
        public string? ResponseCode { get; set; }

        [JsonPropertyName("ResponseDescription")]
        public string? ResponseDescription { get; set; }

        /// <summary>True when <see cref="ResponseCode"/> is "0", meaning Daraja accepted the request for processing.</summary>
        [JsonIgnore]
        public bool IsAccepted => ResponseCode == "0";
    }

    /// <summary>
    /// A single name/value pair inside a callback's ResultParameters or ReferenceData collection.
    /// </summary>
    public class DarajaCallbackParameter
    {
        [JsonPropertyName("Key")]
        public string? Key { get; set; }

        [JsonPropertyName("Value")]
        public object? Value { get; set; }
    }

    /// <summary>
    /// The envelope Daraja posts to your ResultURL/QueueTimeOutURL for B2C, B2B, Reversal,
    /// Transaction Status, and Account Balance requests, wrapped as <c>{ "Result": { ... } }</c>.
    /// </summary>
    public class DarajaResultCallback
    {
        [JsonPropertyName("Result")]
        public DarajaResult? Result { get; set; }
    }

    public class DarajaResult
    {
        [JsonPropertyName("ResultType")]
        public int ResultType { get; set; }

        [JsonPropertyName("ResultCode")]
        public int ResultCode { get; set; }

        [JsonPropertyName("ResultDesc")]
        public string? ResultDesc { get; set; }

        [JsonPropertyName("OriginatorConversationID")]
        public string? OriginatorConversationId { get; set; }

        [JsonPropertyName("ConversationID")]
        public string? ConversationId { get; set; }

        [JsonPropertyName("TransactionID")]
        public string? TransactionId { get; set; }

        [JsonPropertyName("ResultParameters")]
        public DarajaResultParameters? ResultParameters { get; set; }

        [JsonPropertyName("ReferenceData")]
        public DarajaReferenceData? ReferenceData { get; set; }

        /// <summary>True when <see cref="ResultCode"/> is 0.</summary>
        [JsonIgnore]
        public bool IsSuccess => ResultCode == 0;

        /// <summary>Looks up a named entry from <see cref="ResultParameters"/> by key.</summary>
        public object? GetResultParameter(string key)
        {
            var match = ResultParameters?.ResultParameter?.Find(p => p.Key == key);
            return match?.Value;
        }
    }

    public class DarajaResultParameters
    {
        [JsonPropertyName("ResultParameter")]
        public List<DarajaCallbackParameter>? ResultParameter { get; set; }
    }

    public class DarajaReferenceData
    {
        [JsonPropertyName("ReferenceItem")]
        public List<DarajaCallbackParameter>? ReferenceItem { get; set; }
    }
}
