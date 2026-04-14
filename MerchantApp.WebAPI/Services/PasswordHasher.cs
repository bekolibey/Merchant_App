using System.Security.Cryptography;
using System.Text;

namespace MerchantApp.WebAPI.Services;

public static class PasswordHasher
{
    public static string ComputeSha256(string value)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
