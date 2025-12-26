using System.Security.Cryptography;
using System.Text;

namespace IngApp.Infrastructure.Common.Hashing;

public static class Sha256Hash
{
    public static string Hash(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
