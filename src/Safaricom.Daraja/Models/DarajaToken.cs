using System;
using System.Text.Json.Serialization;

namespace Safaricom.Daraja.Models
{
    /// <summary>
    /// An OAuth 2.0 access token issued by Daraja's client-credentials endpoint.
    /// </summary>
    public class DarajaToken
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Daraja returns this as a numeric string (e.g. "3599"), not a JSON number.
        /// </summary>
        [JsonPropertyName("expires_in")]
        public string ExpiresIn { get; set; } = "3599";

        /// <summary>
        /// When this SDK acquired the token, used together with <see cref="ExpiresIn"/>
        /// to compute expiry locally since Daraja does not echo an absolute expiry time.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset AcquiredAt { get; set; }

        public bool IsExpired(int bufferSeconds)
        {
            var lifetimeSeconds = int.TryParse(ExpiresIn, out var parsed) ? parsed : 3599;
            var expiresAt = AcquiredAt.AddSeconds(lifetimeSeconds - bufferSeconds);
            return DateTimeOffset.UtcNow >= expiresAt;
        }
    }
}
