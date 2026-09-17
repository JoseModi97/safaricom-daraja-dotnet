using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Safaricom.Daraja.Security;
using Xunit;

namespace Safaricom.Daraja.Tests;

public class SecurityCredentialEncryptorTests
{
    private static X509Certificate2 CreateSelfSignedTestCertificate(out RSA privateKey)
    {
        privateKey = RSA.Create(2048);
        var request = new CertificateRequest(
            "CN=Safaricom.Daraja Test Certificate",
            privateKey,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        return request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
    }

    [Fact]
    public void EncryptFromCertificate_RoundTripsWithThePrivateKey()
    {
        using var certificate = CreateSelfSignedTestCertificate(out var privateKey);
        using (privateKey)
        {
            const string initiatorPassword = "Safaricom999!*!";

            var encrypted = SecurityCredentialEncryptor.EncryptFromCertificate(certificate, initiatorPassword);
            var encryptedBytes = Convert.FromBase64String(encrypted);
            var decryptedBytes = privateKey.Decrypt(encryptedBytes, RSAEncryptionPadding.Pkcs1);

            Assert.Equal(initiatorPassword, Encoding.UTF8.GetString(decryptedBytes));
        }
    }

    [Fact]
    public void EncryptFromCertificateBytes_AcceptsRawDerEncodedCertificate()
    {
        using var certificate = CreateSelfSignedTestCertificate(out var privateKey);
        using (privateKey)
        {
            var derBytes = certificate.Export(X509ContentType.Cert);
            var encrypted = SecurityCredentialEncryptor.EncryptFromCertificateBytes(derBytes, "SomePassword1");

            Assert.False(string.IsNullOrWhiteSpace(encrypted));
            // Base64 should decode cleanly and be the expected RSA-2048 ciphertext length (256 bytes).
            Assert.Equal(256, Convert.FromBase64String(encrypted).Length);
        }
    }

    [Fact]
    public void EncryptFromCertificate_NullCertificate_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => SecurityCredentialEncryptor.EncryptFromCertificate(null!, "password"));
    }

    [Fact]
    public void EncryptFromCertificate_EmptyPassword_Throws()
    {
        using var certificate = CreateSelfSignedTestCertificate(out var privateKey);
        using (privateKey)
        {
            Assert.Throws<ArgumentException>(() => SecurityCredentialEncryptor.EncryptFromCertificate(certificate, ""));
        }
    }
}
