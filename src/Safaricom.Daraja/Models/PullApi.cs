using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Safaricom.Daraja.Models
{
    public enum PullApiRequestType
    {
        Paybill,
        Till
    }

    /// <summary>Registers a shortcode for the Pull Transactions API, so historical transactions can be pulled on demand.</summary>
    public class PullApiRegisterRequest
    {
        [JsonPropertyName("ShortCode")]
        public string ShortCode { get; set; } = string.Empty;

        [JsonPropertyName("RequestType")]
        public string RequestType { get; set; } = PullApiRequestType.Paybill.ToString();

        [JsonPropertyName("NominatedNumber")]
        public string NominatedNumber { get; set; } = string.Empty;

        [JsonPropertyName("CallBackURL")]
        public string CallBackURL { get; set; } = string.Empty;
    }

    public class PullApiRegisterResponse
    {
        [JsonPropertyName("ResponseCode")]
        public string? ResponseCode { get; set; }

        [JsonPropertyName("ResponseMessage")]
        public string? ResponseMessage { get; set; }
    }

    /// <summary>Pulls settled transactions for a shortcode within a date range, paged via <see cref="OffSetValue"/>.</summary>
    public class PullApiQueryRequest
    {
        [JsonPropertyName("ShortCode")]
        public string ShortCode { get; set; } = string.Empty;

        /// <summary>"yyyy-MM-dd HH:mm:ss".</summary>
        [JsonPropertyName("StartDate")]
        public string StartDate { get; set; } = string.Empty;

        /// <summary>"yyyy-MM-dd HH:mm:ss".</summary>
        [JsonPropertyName("EndDate")]
        public string EndDate { get; set; } = string.Empty;

        [JsonPropertyName("OffSetValue")]
        public string OffSetValue { get; set; } = "0";
    }

    public class PullApiQueryResponse
    {
        [JsonPropertyName("ResponseCode")]
        public string? ResponseCode { get; set; }

        [JsonPropertyName("ResponseMessage")]
        public string? ResponseMessage { get; set; }

        [JsonPropertyName("Response")]
        public List<PullApiTransaction>? Response { get; set; }
    }

    public class PullApiTransaction
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
}
