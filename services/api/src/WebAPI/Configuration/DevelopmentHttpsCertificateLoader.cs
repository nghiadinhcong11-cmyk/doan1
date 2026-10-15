using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace RestaurantPOS.WebAPI.Configuration;

/// <summary>
/// Loads the Development LAN PEM pair through a persisted PKCS#12 object.
/// Windows Schannel cannot use the ephemeral private key produced by
/// CreateFromPemFile for Kestrel server credentials.
/// </summary>
public static class DevelopmentHttpsCertificateLoader
{
    public static X509Certificate2 LoadPersistedPkcs12FromPem(string certificatePath, string privateKeyPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(certificatePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(privateKeyPath);

        using var pemCertificate = X509Certificate2.CreateFromPemFile(certificatePath, privateKeyPath);
        if (!pemCertificate.HasPrivateKey)
            throw new InvalidOperationException("The Development PEM certificate does not contain a private key.");

        var pkcs12Bytes = pemCertificate.Export(X509ContentType.Pkcs12);
        X509Certificate2? persistedCertificate = null;
        try
        {
            persistedCertificate = new X509Certificate2(
                pkcs12Bytes,
                (string?)null,
                X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.PersistKeySet);

            if (!persistedCertificate.HasPrivateKey)
                throw new InvalidOperationException("The persisted Development HTTPS certificate does not contain a private key.");

            return persistedCertificate;
        }
        catch
        {
            persistedCertificate?.Dispose();
            throw;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(pkcs12Bytes);
        }
    }
}
