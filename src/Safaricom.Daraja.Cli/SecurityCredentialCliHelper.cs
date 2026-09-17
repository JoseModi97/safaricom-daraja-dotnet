using System.Collections.Generic;
using Safaricom.Daraja.Security;

namespace Safaricom.Daraja.Cli;

/// <summary>
/// Resolves a SecurityCredential for CLI commands that need one, either from a precomputed
/// value or by encrypting an initiator password with a certificate file on the spot.
/// </summary>
public static class SecurityCredentialCliHelper
{
    public const string UsageHint =
        "Provide either:\n" +
        "  --security-credential <value>          a precomputed SecurityCredential, or\n" +
        "  --cert <path> --initiator-password <p>  a certificate file + password to encrypt now\n" +
        "See the README's SecurityCredential section for where to get your certificate.";

    public static string Resolve(Dictionary<string, string> flags)
    {
        var precomputed = flags.GetValueOrDefault("security-credential");
        if (!string.IsNullOrWhiteSpace(precomputed))
        {
            return precomputed!;
        }

        var certPath = flags.GetValueOrDefault("cert");
        var initiatorPassword = flags.GetValueOrDefault("initiator-password");

        if (string.IsNullOrWhiteSpace(certPath) || string.IsNullOrWhiteSpace(initiatorPassword))
        {
            CliFlags.Fail($"Missing SecurityCredential inputs.\n\n{UsageHint}");
        }

        return SecurityCredentialEncryptor.EncryptFromCertificateFile(certPath!, initiatorPassword!);
    }
}
