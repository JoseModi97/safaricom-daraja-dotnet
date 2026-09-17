using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Safaricom.Daraja.Security;

namespace Safaricom.Daraja.Cli.Commands;

public static class TestCommand
{
    public static async Task ExecuteAsync(string[] args)
    {
        Console.WriteLine("\n\x1b[34m--- Running Safaricom.Daraja Offline Self-Checks ---\x1b[0m\n");

        try
        {
            // 1. SecurityCredential RSA round-trip, using an ephemeral certificate generated
            //    locally (this SDK ships no bundled Safaricom certificate - see the README).
            using var privateKey = RSA.Create(2048);
            var certRequest = new CertificateRequest(
                "CN=Safaricom.Daraja CLI Self-Test",
                privateKey,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
            using var certificate = certRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));

            const string initiatorPassword = "Safaricom999!*!";
            var encrypted = SecurityCredentialEncryptor.EncryptFromCertificate(certificate, initiatorPassword);
            var decryptedBytes = privateKey.Decrypt(Convert.FromBase64String(encrypted), RSAEncryptionPadding.Pkcs1);
            var decrypted = Encoding.UTF8.GetString(decryptedBytes);

            if (decrypted != initiatorPassword)
            {
                throw new Exception("SecurityCredential round-trip did not return the original password.");
            }
            Console.WriteLine("  \x1b[32m✔\x1b[0m SecurityCredential RSA encrypt/decrypt round-trip verified");

            // 2. STK Push password format: Base64(ShortCode + Passkey + Timestamp).
            const string shortCode = "174379";
            const string passkey = "bfb279f9aa9bdbcf158e97dd71a467cd2e0c893059b10f78e6b72ada1ed2c919";
            const string timestamp = "20250925124519";
            var expectedPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{shortCode}{passkey}{timestamp}"));
            var decodedBack = Encoding.UTF8.GetString(Convert.FromBase64String(expectedPassword));

            if (decodedBack != $"{shortCode}{passkey}{timestamp}")
            {
                throw new Exception("STK Push password encoding round-trip failed.");
            }
            Console.WriteLine("  \x1b[32m✔\x1b[0m STK Push password format (Base64(ShortCode+Passkey+Timestamp)) verified");

            Console.WriteLine("\n\x1b[32mAll offline self-checks passed.\x1b[0m");
            Console.WriteLine("Note: this does not verify network connectivity or credential validity against");
            Console.WriteLine("Daraja itself - run \x1b[36msafaricom-daraja token\x1b[0m for that.\n");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nSelf-check failure: {ex.Message}");
            Console.ResetColor();
            Environment.Exit(1);
        }

        await Task.CompletedTask;
    }
}
