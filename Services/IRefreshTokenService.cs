using JobAlertApi.Models;

namespace JobAlertApi.Services
{
    public interface IRefreshTokenService
    {
        string GenerateRefreshToken();
        Task SaveRefreshTokenAsync(int userId, string rawToken);
        Task<RefreshToken?> GetValidRefreshTokenAsync(string rawToken);
        Task RevokeTokenAsync(string rawToken);
    }
}