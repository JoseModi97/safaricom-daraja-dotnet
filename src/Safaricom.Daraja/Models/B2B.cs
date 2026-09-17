using System.Text.Json.Serialization;

namespace Safaricom.Daraja.Models
{
    public enum B2BCommandId
    {
        BusinessPayBill,
        BusinessBuyGoods,
        DisburseFundsToBusiness,
        BusinessToBusinessTransfer,
        MerchantToMerchantTransfer,

        /// <summary>Tax Remittance to KRA. AccountReference must be the KRA Payment Registration Number (PRN).</summary>
        PayTaxes
    }

    /// <summary>Moves funds from one business shortcode to another (PayBill/Buy-Goods/tax remittance).</summary>
    public class B2BRequest
    {
        [JsonPropertyName("Initiator")]
        public string Initiator { get; set; } = string.Empty;

        /// <summary>Build with <see cref="Security.SecurityCredentialEncryptor"/>.</summary>
        [JsonPropertyName("SecurityCredential")]
        public string SecurityCredential { get; set; } = string.Empty;

        [JsonPropertyName("CommandID")]
        public string CommandID { get; set; } = B2BCommandId.BusinessPayBill.ToString();

        [JsonPropertyName("SenderIdentifierType")]
        public string SenderIdentifierType { get; set; } = ((int)DarajaIdentifierType.Shortcode).ToString();

        [JsonPropertyName("RecieverIdentifierType")]
        public string RecieverIdentifierType { get; set; } = ((int)DarajaIdentifierType.Shortcode).ToString();

        [JsonPropertyName("Amount")]
        public string Amount { get; set; } = string.Empty;

        [JsonPropertyName("PartyA")]
        public string PartyA { get; set; } = string.Empty;

        [JsonPropertyName("PartyB")]
        public string PartyB { get; set; } = string.Empty;

        /// <summary>For <see cref="B2BCommandId.PayTaxes"/>, set this to the KRA Payment Registration Number (PRN).</summary>
        [JsonPropertyName("AccountReference")]
        public string AccountReference { get; set; } = string.Empty;

        [JsonPropertyName("Remarks")]
        public string Remarks { get; set; } = string.Empty;

        [JsonPropertyName("QueueTimeOutURL")]
        public string QueueTimeOutURL { get; set; } = string.Empty;

        [JsonPropertyName("ResultURL")]
        public string ResultURL { get; set; } = string.Empty;
    }
}
