using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Safaricom.Daraja.IoT.Models
{
    public class AllSimsRequest
    {
        [JsonPropertyName("vpnGroup")]
        public List<string> VpnGroup { get; set; } = new List<string>();

        [JsonPropertyName("startAtIndex")]
        public string StartAtIndex { get; set; } = "0";

        [JsonPropertyName("pageSize")]
        public string PageSize { get; set; } = "0";

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
    }

    public class QueryLifeCycleStatusRequest
    {
        [JsonPropertyName("msisdn")]
        public string Msisdn { get; set; } = string.Empty;

        [JsonPropertyName("vpnGroup")]
        public string VpnGroup { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
    }

    public class QueryCustomerInfoRequest
    {
        [JsonPropertyName("msisdn")]
        public string Msisdn { get; set; } = string.Empty;

        [JsonPropertyName("vpnGroup")]
        public string VpnGroup { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
    }

    public class SimActivationRequest
    {
        [JsonPropertyName("msisdn")]
        public string Msisdn { get; set; } = string.Empty;

        [JsonPropertyName("vpnGroup")]
        public string VpnGroup { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
    }

    public class GetActivationTrendsRequest
    {
        [JsonPropertyName("vpnGroup")]
        public string VpnGroup { get; set; } = string.Empty;

        [JsonPropertyName("startDate")]
        public string StartDate { get; set; } = string.Empty;

        [JsonPropertyName("stopDate")]
        public string StopDate { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
    }

    public class RenameAssetRequest
    {
        [JsonPropertyName("msisdn")]
        public string Msisdn { get; set; } = string.Empty;

        [JsonPropertyName("vpnGroup")]
        public string VpnGroup { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("assetName")]
        public string AssetName { get; set; } = string.Empty;
    }

    public class GetLocationInfoRequest
    {
        [JsonPropertyName("msisdn")]
        public string Msisdn { get; set; } = string.Empty;

        [JsonPropertyName("vpnGroup")]
        public string VpnGroup { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
    }

    /// <summary>The operation to apply in <see cref="SuspendUnsuspendSubRequest"/>.</summary>
    public enum SubscriberOperation
    {
        Suspend,
        Unsuspend
    }

    public class SuspendUnsuspendSubRequest
    {
        [JsonPropertyName("msisdn")]
        public string Msisdn { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("vpnGroup")]
        public string VpnGroup { get; set; } = string.Empty;

        [JsonPropertyName("product")]
        public string Product { get; set; } = string.Empty;

        [JsonPropertyName("operation")]
        public string Operation { get; set; } = SubscriberOperation.Suspend.ToString();
    }
}
