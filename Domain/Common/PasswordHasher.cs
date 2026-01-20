using System.Security.Cryptography;
using System.Text;

namespace MyShopServer.Domain.Common;

public static class PasswordHasher
{
    public static string Hash(string password)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }
}
