using Microsoft.AspNetCore.Identity;

namespace RestaurantPOS.Application.Common.Security;

public readonly record struct PasswordVerificationOutcome(bool IsValid, bool NeedsRehash);

public static class LegacyPasswordVerifier
{
    public static PasswordVerificationOutcome Verify<TUser>(
        IPasswordHasher<TUser> hasher,
        TUser user,
        string storedPassword,
        string providedPassword)
        where TUser : class
    {
        try
        {
            var result = hasher.VerifyHashedPassword(user, storedPassword, providedPassword);
            return result switch
            {
                PasswordVerificationResult.Success => new(true, false),
                PasswordVerificationResult.SuccessRehashNeeded => new(true, true),
                _ => LegacyPlaintextMatch(storedPassword, providedPassword)
            };
        }
        catch (FormatException)
        {
            return LegacyPlaintextMatch(storedPassword, providedPassword);
        }
    }

    private static PasswordVerificationOutcome LegacyPlaintextMatch(string storedPassword, string providedPassword) =>
        storedPassword == providedPassword
            ? new(true, true)
            : new(false, false);
}
