using System.Security.Cryptography;
using System.Text;
using JobAlertApi.Data;
using JobAlertApi.Models;
using Microsoft.EntityFrameworkCore;

namespace JobAlertApi.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly AppDbContext _context;

        public RefreshTokenService(AppDbContext context)
        {
            _context = context;
        }

        // ===============================
        // Generate RAW refresh token
        // ===============================
        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];

            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            return Convert.ToBase64String(randomBytes);
        }

        // ===============================
        // HASH token (SHA256)
        // ===============================
        private string HashToken(string token)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }

        // ===============================
        // 💾 Save refresh token (HASHED)
        // ===============================
        public async Task SaveRefreshTokenAsync(int userId, string rawToken)
        {
            var hashedToken = HashToken(rawToken);

            var refreshToken = new RefreshToken
            {
                TokenHash = hashedToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                UserId = userId,
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
        }

        // ===============================
        // 🔍 Validate refresh token
        // ===============================
        public async Task<RefreshToken?> GetValidRefreshTokenAsync(string rawToken)
        {
            var hashedToken = HashToken(rawToken);

            return await _context.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.TokenHash == hashedToken &&
                    !x.IsRevoked &&
                    x.ExpiresAt > DateTime.UtcNow);
        }

        // ===============================
        // Revoke token
        // ===============================
        public async Task RevokeTokenAsync(string rawToken)
        {
            var hashedToken = HashToken(rawToken);

            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.TokenHash == hashedToken);

            if (token != null)
            {
                token.IsRevoked = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}