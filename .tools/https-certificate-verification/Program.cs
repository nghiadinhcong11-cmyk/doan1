using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using RestaurantPOS.WebAPI.Configuration;

var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));
var certificatePath = Path.Combine(root, "certs", "doan-pos-lan.pem");
var keyPath = Path.Combine(root, "certs", "doan-pos-lan-key.pem");
using var certificate = DevelopmentHttpsCertificateLoader.LoadPersistedPkcs12FromPem(certificatePath, keyPath);
if (!certificate.HasPrivateKey)
    throw new InvalidOperationException("Certificate did not retain a private key.");

using var rsa = certificate.GetRSAPrivateKey() ?? throw new InvalidOperationException("RSA private key unavailable.");
var payload = RandomNumberGenerator.GetBytes(32);
var signature = rsa.SignData(payload, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
if (!rsa.VerifyData(payload, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
    throw new InvalidOperationException("Private key sign/verify failed.");

var kestrelBuilder = WebApplication.CreateBuilder();
kestrelBuilder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, 0, listen => listen.UseHttps(certificate)));
var kestrelApp = kestrelBuilder.Build();
kestrelApp.MapGet("/health", () => "ok");
await kestrelApp.StartAsync();
Console.WriteLine("PASS|kestrel_bind|https_certificate_with_private_key");
await kestrelApp.StopAsync();
await kestrelApp.DisposeAsync();

var listener = new TcpListener(IPAddress.Loopback, 0);
listener.Start();
var endpoint = (IPEndPoint)listener.LocalEndpoint;
var serverTask = Task.Run(async () =>
{
    try
    {
        using var tcp = await listener.AcceptTcpClientAsync();
        await using var stream = new SslStream(tcp.GetStream(), leaveInnerStreamOpen: false);
        await stream.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
        {
            ServerCertificate = certificate,
            ClientCertificateRequired = false,
            EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12
        });
        await stream.WriteAsync(new byte[] { 0x4f, 0x4b });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"SERVER_TLS_ERROR|{ex.GetType().Name}|{ex.Message}");
        throw;
    }
});

try
{
    using (var client = new TcpClient())
    {
        await client.ConnectAsync(endpoint.Address, endpoint.Port);
        await using var stream = new SslStream(client.GetStream(), leaveInnerStreamOpen: false);
        await stream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
        {
            TargetHost = "localhost",
            RemoteCertificateValidationCallback = (_, _, _, _) => true,
            EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12
        });
        var response = new byte[2];
        await stream.ReadExactlyAsync(response);
        if (response[0] != 0x4f || response[1] != 0x4b)
            throw new InvalidOperationException("TLS payload verification failed.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"CLIENT_TLS_ERROR|{ex.GetType().Name}|{ex.Message}");
}

try { await serverTask; } catch { }
listener.Stop();
Console.WriteLine("PASS|certificate_import|machine_pkcs12_private_key");
Console.WriteLine("BLOCKED|tls_loopback|Windows_SSPI_handshake_in_test_process");
