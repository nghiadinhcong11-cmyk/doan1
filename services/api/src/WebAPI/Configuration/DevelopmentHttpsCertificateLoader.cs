using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace RestaurantPOS.WebAPI.Configuration;

/// <summary>
/// Loads the Development LAN PEM pair through a temporary PKCS#12 object.
/// The current Windows account cannot import this key with UserKeySet
/// (the import fails with Access denied). Schannel also cannot serve TLS from
/// an EphemeralKeySet key on this runtime, so MachineKeySet with
/// PersistKeySet and Exportable are required for Schannel to retain usable server
/// credentials. This avoids the inaccessible user key store and does not
/// change the PEM files or install a certificate into the certificate store.
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
                X509KeyStorageFlags.MachineKeySet |
                X509KeyStorageFlags.PersistKeySet |
                X509KeyStorageFlags.Exportable);

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
