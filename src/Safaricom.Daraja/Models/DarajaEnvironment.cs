namespace Safaricom.Daraja.Models
{
    /// <summary>
    /// Target Daraja environment: Sandbox (developer testing) or Production (live operations).
    /// </summary>
    public enum DarajaEnvironment
    {
        /// <summary>Developer sandbox environment at sandbox.safaricom.co.ke.</summary>
        Sandbox,

        /// <summary>Live production environment at api.safaricom.co.ke.</summary>
        Production
    }

    /// <summary>
    /// Resolves the base API host for a given <see cref="DarajaEnvironment"/>.
    /// </summary>
    public static class DarajaEndpoints
    {
        public const string SandboxBaseUrl = "https://sandbox.safaricom.co.ke";
        public const string ProductionBaseUrl = "https://api.safaricom.co.ke";

        public static string GetBaseAddress(DarajaEnvironment environment)
        {
            return environment == DarajaEnvironment.Production ? ProductionBaseUrl : SandboxBaseUrl;
        }
    }
}
