using System.Text.Json.Serialization;

namespace Safaricom.Daraja.Models
{
    /// <summary>Queries the account balance of a shortcode.</summary>
    public class AccountBalanceRequest
    {
        [JsonPropertyName("Initiator")]
        public string Initiator { get; set; } = string.Empty;

        /// <summary>Build with <see cref="Security.SecurityCredentialEncryptor"/>.</summary>
        [JsonPropertyName("SecurityCredential")]
        public string SecurityCredential { get; set; } = string.Empty;

        [JsonPropertyName("CommandID")]
        public string CommandID { get; set; } = "AccountBalance";

        [JsonPropertyName("PartyA")]
        public string PartyA { get; set; } = string.Empty;

        [JsonPropertyName("IdentifierType")]
        public string IdentifierType { get; set; } = ((int)DarajaIdentifierType.Shortcode).ToString();

        [JsonPropertyName("Remarks")]
        public string Remarks { get; set; } = string.Empty;

        [JsonPropertyName("QueueTimeOutURL")]
        public string QueueTimeOutURL { get; set; } = string.Empty;

        [JsonPropertyName("ResultURL")]
        public string ResultURL { get; set; } = string.Empty;
    }
}
