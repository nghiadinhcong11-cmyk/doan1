using System.Security.Cryptography;

namespace RestaurantPOS.Application.Common.Security;

public static class QrTokenGenerator
{
    public const int RandomByteCount = 32;
    public const int EncodedLength = 43;

    public static string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(RandomByteCount);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
