using System;

namespace JobAlertApi.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public string TokenHash { get; set; } = null!;
        public string TokenSalt { get; set; } = null!;
        

        // 🔐 Expiration Date
        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt {  get; set; }= DateTime.UtcNow;

        // 🔁 Revocation flag
        public bool IsRevoked { get; set; } = false;

        // 🔗 Foreign key
        public int UserId { get; set; }

        public User User { get; set; }=null!;
    }
}