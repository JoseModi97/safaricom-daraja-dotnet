using System.Text.Json.Serialization;

namespace Safaricom.Daraja.IoT.Models
{
    public class SearchMessagesRequest
    {
        [JsonPropertyName("searchValue")]
        public string SearchValue { get; set; } = string.Empty;

        [JsonPropertyName("vpnGroup")]
        public string VpnGroup { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
    }

    public class FilterMessagesRequest
    {
        [JsonPropertyName("startDate")]
        public string StartDate { get; set; } = string.Empty;

        [JsonPropertyName("endDate")]
        public string EndDate { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("vpnGroup")]
        public string VpnGroup { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
    }

    public class DeleteMessageThreadRequest
    {
        [JsonPropertyName("msisdn")]
        public string Msisdn { get; set; } = string.Empty;

        [JsonPropertyName("vpnGroup")]
        public string VpnGroup { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
    }

    public class GetAllMessagesRequest
    {
        [JsonPropertyName("vpnGroup")]
        public string VpnGroup { get; set; } = string.Empty;
    }

    public class SendSingleMessageRequest
    {
        [JsonPropertyName("msisdn")]
        public string Msisdn { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("vpnGroup")]
        public string VpnGroup { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
    }

    public class DeleteMessageRequest
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("vpnGroup")]
        public string VpnGroup { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
    }
}
