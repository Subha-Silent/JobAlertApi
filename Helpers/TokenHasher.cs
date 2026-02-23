using System.Security.Cryptography;
using System.Text;

namespace JobAlertApi.Helpers
{
    public static class TokenHasher
    {
        public static (string hash, string salt) HashToken(string token)
        {
            using var hmac = new HMACSHA256();

            var salt = Convert.ToBase64String(hmac.Key);
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(token));
            var hash = Convert.ToBase64String(hashBytes);

            return (hash, salt);
        }

        public static bool VerifyToken(string token, string storedHash, string storedSalt)
        {
            var key = Convert.FromBase64String(storedSalt);

            using var hmac = new HMACSHA256(key);

            var computedHash = Convert.ToBase64String(
                hmac.ComputeHash(Encoding.UTF8.GetBytes(token)));

            return computedHash == storedHash;
        }
    }
}