using System.Text.Json.Serialization;

namespace Safaricom.Daraja.Models
{
    /// <summary>Redeems a customer's Bonga points against a Paybill payment.</summary>
    public class BongaRedeemPaybillRequest
    {
        [JsonPropertyName("msisdn")]
        public string Msisdn { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("bongaPoints")]
        public decimal BongaPoints { get; set; }

        [JsonPropertyName("conversionRate")]
        public decimal ConversionRate { get; set; } = 0.2m;

        [JsonPropertyName("shortCode")]
        public string ShortCode { get; set; } = string.Empty;

        [JsonPropertyName("accountNumber")]
        public string AccountNumber { get; set; } = string.Empty;
    }

    /// <summary>Converts a Bonga points count into its M-Pesa monetary equivalent.</summary>
    public class BongaCalculatePointsRequest
    {
        [JsonPropertyName("points")]
        public string Points { get; set; } = string.Empty;
    }

    /// <remarks>
    /// Lipa na Bonga is not part of Safaricom's public Daraja API catalog, so its response
    /// schema is unconfirmed beyond what your own sandbox testing shows. This SDK returns the
    /// raw response as <see cref="DarajaRawResult"/> rather than a fabricated typed shape —
    /// inspect <see cref="DarajaRawResult.Json"/> and, once you've confirmed the real field
    /// names against your sandbox, tell the SDK maintainers so a typed model can be added.
    /// </remarks>
    public class DarajaRawResult
    {
        /// <summary>The exact response body returned by Daraja for a successful (2xx) call.</summary>
        public string Json { get; set; } = string.Empty;
    }
}
