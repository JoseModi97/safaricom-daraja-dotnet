using System;

namespace Safaricom.Daraja.Models
{
    /// <summary>
    /// Configuration options for connecting to the Safaricom Daraja API.
    /// </summary>
    public class DarajaConfig
    {
        /// <summary>
        /// The target environment (Sandbox or Production). Defaults to Sandbox so
        /// that misconfigured apps fail safe against test money rather than live money.
        /// </summary>
        public DarajaEnvironment Environment { get; set; } = DarajaEnvironment.Sandbox;

        /// <summary>
        /// Registered application Consumer Key obtained from the Daraja developer portal.
        /// </summary>
        public string? ConsumerKey { get; set; }

        /// <summary>
        /// Registered application Consumer Secret.
        /// </summary>
        public string? ConsumerSecret { get; set; }

        /// <summary>
        /// Overrides the computed base address (e.g. for testing against a proxy or mock server).
        /// Leave blank to use the standard Sandbox/Production host for <see cref="Environment"/>.
        /// </summary>
        public string? BaseAddress { get; set; }

        /// <summary>
        /// HTTP request timeout. Defaults to 60 seconds.
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Seconds subtracted from the OAuth token's reported lifetime before it is
        /// considered expired, to avoid racing a token that expires mid-request.
        /// </summary>
        public int TokenExpiryBufferSeconds { get; set; } = 60;

        public DarajaConfig()
        {
            var envConsumerKey = System.Environment.GetEnvironmentVariable("DARAJA_CONSUMER_KEY");
            if (!string.IsNullOrWhiteSpace(envConsumerKey)) ConsumerKey = envConsumerKey;

            var envConsumerSecret = System.Environment.GetEnvironmentVariable("DARAJA_CONSUMER_SECRET");
            if (!string.IsNullOrWhiteSpace(envConsumerSecret)) ConsumerSecret = envConsumerSecret;

            var envEnvironment = System.Environment.GetEnvironmentVariable("DARAJA_ENVIRONMENT");
            if (!string.IsNullOrWhiteSpace(envEnvironment) && Enum.TryParse<DarajaEnvironment>(envEnvironment, true, out var parsedEnv))
            {
                Environment = parsedEnv;
            }

            var envBaseAddress = System.Environment.GetEnvironmentVariable("DARAJA_BASE_ADDRESS");
            if (!string.IsNullOrWhiteSpace(envBaseAddress)) BaseAddress = envBaseAddress;
        }

        /// <summary>
        /// Resolves the effective base address: the explicit override if set, otherwise
        /// the standard host for <see cref="Environment"/>.
        /// </summary>
        public string GetEffectiveBaseAddress()
        {
            return !string.IsNullOrWhiteSpace(BaseAddress)
                ? BaseAddress!.TrimEnd('/')
                : DarajaEndpoints.GetBaseAddress(Environment);
        }

        /// <summary>
        /// Validates that the minimum credentials required to authenticate exist.
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ConsumerKey) || string.IsNullOrWhiteSpace(ConsumerSecret))
            {
                throw new InvalidOperationException(
                    "'ConsumerKey' and 'ConsumerSecret' must be provided in DarajaConfig (or via the " +
                    "DARAJA_CONSUMER_KEY / DARAJA_CONSUMER_SECRET environment variables).");
            }
        }
    }
}
