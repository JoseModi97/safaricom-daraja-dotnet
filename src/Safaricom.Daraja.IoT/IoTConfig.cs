using System;
using Safaricom.Daraja.Models;

namespace Safaricom.Daraja.IoT
{
    /// <summary>
    /// Configuration for the Safaricom IoT SIM-portal / IMSI APIs (<c>/simportal/v1/*</c> and
    /// <c>/imsi/v{1,2}/checkATI</c>). These share a host with core Daraja but use a different
    /// authentication and header scheme — see the README for what is and isn't confirmed here.
    /// </summary>
    public class IoTConfig
    {
        /// <summary>The target environment. Shares the same Sandbox/Production hosts as core Daraja.</summary>
        public DarajaEnvironment Environment { get; set; } = DarajaEnvironment.Sandbox;

        /// <summary>Overrides the computed base address.</summary>
        public string? BaseAddress { get; set; }

        /// <summary>
        /// A pre-acquired Bearer access token, if you obtain one outside this SDK. Leave blank
        /// and set <see cref="ConsumerKey"/>/<see cref="ConsumerSecret"/> instead to have this
        /// module fetch and cache one for you via the same <c>/oauth/v1/generate</c> endpoint
        /// core Daraja uses — this SDK cannot confirm that endpoint issues valid tokens for the
        /// IoT product specifically, since the exported reference collection this module is
        /// built from left the token acquisition step blank. Try it, and if it doesn't work for
        /// your app registration, supply <see cref="AccessToken"/> directly instead.
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>Consumer Key for this app's IoT/SIM-portal registration, if using automatic token acquisition.</summary>
        public string? ConsumerKey { get; set; }

        /// <summary>Consumer Secret for this app's IoT/SIM-portal registration, if using automatic token acquisition.</summary>
        public string? ConsumerSecret { get; set; }

        /// <summary>The <c>x-api-key</c> header value issued for your IoT/SIM-portal app.</summary>
        public string? ApiKey { get; set; }

        /// <summary>The <c>X-MSISDN</c> header: the identifying number of the calling system/operator.</summary>
        public string? Msisdn { get; set; }

        /// <summary>The <c>x-source-system</c> header. Defaults to "web-portal", matching the reference collection.</summary>
        public string SourceSystem { get; set; } = "web-portal";

        /// <summary>The <c>X-App</c> header. Defaults to "web-portal", matching the reference collection.</summary>
        public string App { get; set; } = "web-portal";

        /// <summary>The <c>Accept-Language</c> header. Defaults to "EN".</summary>
        public string? AcceptLanguage { get; set; } = "EN";

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);

        public IoTConfig()
        {
            var envApiKey = System.Environment.GetEnvironmentVariable("DARAJA_IOT_API_KEY");
            if (!string.IsNullOrWhiteSpace(envApiKey)) ApiKey = envApiKey;

            var envAccessToken = System.Environment.GetEnvironmentVariable("DARAJA_IOT_ACCESS_TOKEN");
            if (!string.IsNullOrWhiteSpace(envAccessToken)) AccessToken = envAccessToken;

            var envConsumerKey = System.Environment.GetEnvironmentVariable("DARAJA_IOT_CONSUMER_KEY");
            if (!string.IsNullOrWhiteSpace(envConsumerKey)) ConsumerKey = envConsumerKey;

            var envConsumerSecret = System.Environment.GetEnvironmentVariable("DARAJA_IOT_CONSUMER_SECRET");
            if (!string.IsNullOrWhiteSpace(envConsumerSecret)) ConsumerSecret = envConsumerSecret;

            var envMsisdn = System.Environment.GetEnvironmentVariable("DARAJA_IOT_MSISDN");
            if (!string.IsNullOrWhiteSpace(envMsisdn)) Msisdn = envMsisdn;
        }

        public string GetEffectiveBaseAddress()
        {
            return !string.IsNullOrWhiteSpace(BaseAddress)
                ? BaseAddress!.TrimEnd('/')
                : DarajaEndpoints.GetBaseAddress(Environment);
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                throw new InvalidOperationException("'ApiKey' (the x-api-key header value) must be provided in IoTConfig.");
            }

            if (string.IsNullOrWhiteSpace(AccessToken) &&
                (string.IsNullOrWhiteSpace(ConsumerKey) || string.IsNullOrWhiteSpace(ConsumerSecret)))
            {
                throw new InvalidOperationException(
                    "Either 'AccessToken' or both 'ConsumerKey' and 'ConsumerSecret' must be provided in IoTConfig.");
            }
        }
    }
}
