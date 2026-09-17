using System.Text.Json.Serialization;

namespace Safaricom.Daraja.Models
{
    /// <summary>Queries the status of a previous M-Pesa transaction.</summary>
    public class TransactionStatusRequest
    {
        [JsonPropertyName("Initiator")]
        public string Initiator { get; set; } = string.Empty;

        /// <summary>Build with <see cref="Security.SecurityCredentialEncryptor"/>.</summary>
        [JsonPropertyName("SecurityCredential")]
        public string SecurityCredential { get; set; } = string.Empty;

        [JsonPropertyName("CommandID")]
        public string CommandID { get; set; } = "TransactionStatusQuery";

        [JsonPropertyName("TransactionID")]
        public string TransactionID { get; set; } = string.Empty;

        [JsonPropertyName("PartyA")]
        public string PartyA { get; set; } = string.Empty;

        [JsonPropertyName("IdentifierType")]
        public string IdentifierType { get; set; } = ((int)DarajaIdentifierType.Shortcode).ToString();

        [JsonPropertyName("ResultURL")]
        public string ResultURL { get; set; } = string.Empty;

        [JsonPropertyName("QueueTimeOutURL")]
        public string QueueTimeOutURL { get; set; } = string.Empty;

        [JsonPropertyName("Remarks")]
        public string Remarks { get; set; } = string.Empty;

        [JsonPropertyName("Occasion")]
        public string? Occasion { get; set; }
    }
}
