using System.Text.Json.Serialization;

namespace Safaricom.Daraja.Models
{
    /// <summary>How often an M-Pesa Ratiba standing order recurs.</summary>
    public enum RatibaFrequency
    {
        OneOff = 1,
        Daily = 2,
        Weekly = 3,
        Monthly = 4,
        BiMonthly = 5,
        Quarterly = 6,
        HalfYear = 7,
        Yearly = 8
    }

    /// <summary>Whether the standing order pays a Paybill or a Buy Goods (till) merchant.</summary>
    public enum RatibaReceiverType
    {
        /// <summary>Paybill. Wire value "4".</summary>
        Paybill = 4,

        /// <summary>Buy Goods / till. Wire value "2".</summary>
        BuyGoods = 2
    }

    /// <summary>Creates a recurring M-Pesa Ratiba standing order authorized by the customer.</summary>
    public class RatibaCreateRequest
    {
        [JsonPropertyName("StandingOrderName")]
        public string StandingOrderName { get; set; } = string.Empty;

        [JsonPropertyName("BusinessShortCode")]
        public string BusinessShortCode { get; set; } = string.Empty;

        /// <summary>"Standing Order Customer Pay Bill" or "Standing Order Customer Pay Merchant".</summary>
        [JsonPropertyName("TransactionType")]
        public string TransactionType { get; set; } = "Standing Order Customer Pay Bill";

        [JsonPropertyName("Amount")]
        public string Amount { get; set; } = string.Empty;

        /// <summary>The customer's MSISDN authorizing the standing order.</summary>
        [JsonPropertyName("PartyA")]
        public string PartyA { get; set; } = string.Empty;

        [JsonPropertyName("ReceiverPartyIdentifierType")]
        public string ReceiverPartyIdentifierType { get; set; } = ((int)RatibaReceiverType.Paybill).ToString();

        [JsonPropertyName("CallBackURL")]
        public string CallBackURL { get; set; } = string.Empty;

        [JsonPropertyName("AccountReference")]
        public string AccountReference { get; set; } = string.Empty;

        [JsonPropertyName("TransactionDesc")]
        public string TransactionDesc { get; set; } = string.Empty;

        [JsonPropertyName("Frequency")]
        public string Frequency { get; set; } = ((int)RatibaFrequency.Monthly).ToString();

        /// <summary>yyyy-MM-dd.</summary>
        [JsonPropertyName("StartDate")]
        public string StartDate { get; set; } = string.Empty;

        /// <summary>yyyy-MM-dd.</summary>
        [JsonPropertyName("EndDate")]
        public string EndDate { get; set; } = string.Empty;
    }

    /// <remarks>
    /// Ratiba is one of Daraja's newer products and its response field casing is not as
    /// consistently documented across Safaricom's own materials as the core transaction APIs.
    /// This model reflects the commonly observed shape; treat unfamiliar fields defensively.
    /// </remarks>
    public class RatibaCreateResponse
    {
        [JsonPropertyName("ResponseCode")]
        public string? ResponseCode { get; set; }

        [JsonPropertyName("ResponseDescription")]
        public string? ResponseDescription { get; set; }

        [JsonIgnore]
        public bool IsAccepted => ResponseCode == "0";
    }
}
