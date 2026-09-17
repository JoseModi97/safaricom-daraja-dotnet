using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Safaricom.Daraja.Security
{
    /// <summary>
    /// Encrypts an Initiator password into the <c>SecurityCredential</c> value required by
    /// B2C, B2B, Reversal, and Account Balance requests.
    /// </summary>
    /// <remarks>
    /// Daraja requires the initiator password to be RSA-encrypted (PKCS#1 v1.5 padding) with
    /// Safaricom's public certificate, then base64-encoded. <b>Sandbox and Production use
    /// different certificates</b>, and both must be downloaded from the Daraja developer
    /// portal (Docs &gt; APIs &gt; Authorization) — this SDK does not bundle either certificate,
    /// since third-party copies circulating online have in the past turned out to be the
    /// wrong (G2) certificate and silently produce credentials Daraja rejects. Always source
    /// your <c>.cer</c> file directly from https://developer.safaricom.co.ke.
    /// </remarks>
    public static class SecurityCredentialEncryptor
    {
        /// <summary>
        /// Encrypts <paramref name="initiatorPassword"/> using the public key embedded in the
        /// given X.509 certificate file (DER or PEM encoded <c>.cer</c>/<c>.pem</c>).
        /// </summary>
        public static string EncryptFromCertificateFile(string certificatePath, string initiatorPassword)
        {
            if (string.IsNullOrWhiteSpace(certificatePath)) throw new ArgumentException("Certificate path is required.", nameof(certificatePath));
            var certificateBytes = File.ReadAllBytes(certificatePath);
            return EncryptFromCertificateBytes(certificateBytes, initiatorPassword);
        }

        /// <summary>
        /// Encrypts <paramref name="initiatorPassword"/> using the public key embedded in the
        /// given raw X.509 certificate bytes.
        /// </summary>
        public static string EncryptFromCertificateBytes(byte[] certificateBytes, string initiatorPassword)
        {
            if (certificateBytes == null || certificateBytes.Length == 0) throw new ArgumentException("Certificate bytes are required.", nameof(certificateBytes));

#if NET9_0_OR_GREATER
            using var certificate = X509CertificateLoader.LoadCertificate(certificateBytes);
#else
            using var certificate = new X509Certificate2(certificateBytes);
#endif
            return EncryptFromCertificate(certificate, initiatorPassword);
        }

        /// <summary>
        /// Encrypts <paramref name="initiatorPassword"/> using an already-loaded certificate.
        /// </summary>
        public static string EncryptFromCertificate(X509Certificate2 certificate, string initiatorPassword)
        {
            if (certificate == null) throw new ArgumentNullException(nameof(certificate));
            if (string.IsNullOrWhiteSpace(initiatorPassword)) throw new ArgumentException("Initiator password is required.", nameof(initiatorPassword));

            using var rsa = certificate.GetRSAPublicKey()
                ?? throw new InvalidOperationException("The supplied certificate does not contain an RSA public key.");

            var plainBytes = Encoding.UTF8.GetBytes(initiatorPassword);
            var encryptedBytes = rsa.Encrypt(plainBytes, RSAEncryptionPadding.Pkcs1);
            return Convert.ToBase64String(encryptedBytes);
        }
    }
}
