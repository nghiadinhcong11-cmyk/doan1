namespace RestaurantPOS.Application.Common.Security;

public static class PasswordPolicy
{
    public const int MinimumLength = 8;
    public const int MaximumLength = 128;

    public static string? Validate(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return "Mật khẩu không được để trống.";

        if (password.Length < MinimumLength)
            return $"Mật khẩu phải có ít nhất {MinimumLength} ký tự.";

        if (password.Length > MaximumLength)
            return $"Mật khẩu không được vượt quá {MaximumLength} ký tự.";

        return null;
    }
}
