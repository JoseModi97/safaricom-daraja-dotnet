using System.Text.Json.Serialization;

namespace Safaricom.Daraja.Models
{
    /// <summary>Reverses a previously completed M-Pesa transaction.</summary>
    public class ReversalRequest
    {
        [JsonPropertyName("Initiator")]
        public string Initiator { get; set; } = string.Empty;

        /// <summary>Build with <see cref="Security.SecurityCredentialEncryptor"/>.</summary>
        [JsonPropertyName("SecurityCredential")]
        public string SecurityCredential { get; set; } = string.Empty;

        [JsonPropertyName("CommandID")]
        public string CommandID { get; set; } = "TransactionReversal";

        [JsonPropertyName("TransactionID")]
        public string TransactionID { get; set; } = string.Empty;

        [JsonPropertyName("Amount")]
        public string Amount { get; set; } = string.Empty;

        [JsonPropertyName("ReceiverParty")]
        public string ReceiverParty { get; set; } = string.Empty;

        [JsonPropertyName("RecieverIdentifierType")]
        public string RecieverIdentifierType { get; set; } = ((int)DarajaIdentifierType.Shortcode).ToString();

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
