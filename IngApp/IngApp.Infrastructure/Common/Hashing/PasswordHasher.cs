using System;
using System.Security.Cryptography;
using System.Text;

namespace IngApp.Infrastructure.Common.Hashing;

/// <summary>
/// Service برای Hash کردن و Verify کردن Password
/// از BCrypt استفاده می‌کند
/// </summary>
public static class PasswordHasher
{
    /// <summary>
    /// Hash کردن Password
    /// </summary>
    public static string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        // استفاده از BCrypt.Net-Next
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
    }

    /// <summary>
    /// بررسی اینکه Password با Hash مطابقت دارد یا نه
    /// </summary>
    public static bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            return false;
        }
    }
}












