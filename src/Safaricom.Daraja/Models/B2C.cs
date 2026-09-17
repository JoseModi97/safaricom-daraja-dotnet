using System.Text.Json.Serialization;

namespace Safaricom.Daraja.Models
{
    public enum B2CCommandId
    {
        SalaryPayment,
        BusinessPayment,
        PromotionPayment
    }

    /// <summary>Pays out from a business shortcode to a customer MSISDN.</summary>
    public class B2CRequest
    {
        [JsonPropertyName("InitiatorName")]
        public string InitiatorName { get; set; } = string.Empty;

        /// <summary>Build with <see cref="Security.SecurityCredentialEncryptor"/>.</summary>
        [JsonPropertyName("SecurityCredential")]
        public string SecurityCredential { get; set; } = string.Empty;

        [JsonPropertyName("CommandID")]
        public string CommandID { get; set; } = B2CCommandId.BusinessPayment.ToString();

        [JsonPropertyName("Amount")]
        public string Amount { get; set; } = string.Empty;

        /// <summary>The paying business shortcode.</summary>
        [JsonPropertyName("PartyA")]
        public string PartyA { get; set; } = string.Empty;

        /// <summary>The receiving customer MSISDN.</summary>
        [JsonPropertyName("PartyB")]
        public string PartyB { get; set; } = string.Empty;

        [JsonPropertyName("Remarks")]
        public string Remarks { get; set; } = string.Empty;

        [JsonPropertyName("QueueTimeOutURL")]
        public string QueueTimeOutURL { get; set; } = string.Empty;

        [JsonPropertyName("ResultURL")]
        public string ResultURL { get; set; } = string.Empty;

        [JsonPropertyName("Occasion")]
        public string? Occasion { get; set; }

        /// <summary>Required by the B2Pochi (business-to-Pochi-la-Biashara) variant of this same endpoint.</summary>
        [JsonPropertyName("OriginatorConversationID")]
        public string? OriginatorConversationId { get; set; }
    }
}
