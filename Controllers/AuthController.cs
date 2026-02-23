using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JobAlertApi.Data;
using JobAlertApi.Models;
using JobAlertApi.Services;
using JobAlertApi.Helpers;

namespace JobAlertApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IRefreshTokenService _refreshTokenService;

        public AuthController(
            AppDbContext context,
            IConfiguration config,
            IRefreshTokenService refreshTokenService)
        {
            _context = context;
            _config = config;
            _refreshTokenService = refreshTokenService;
        }

        // ================================
        // REGISTER
        // ================================
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] AuthRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid request" });

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest(new { message = "User already exists" });

            var user = new User
            {
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully" });
        }

        // ================================
        // LOGIN (SECURE HASHED VERSION)
        // ================================
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] AuthRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid request" });

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid credentials" });

            // Generate access token
            var accessToken = GenerateJwtToken(user);

            // Generate RAW refresh token (client will receive this)
            var rawRefreshToken = _refreshTokenService.GenerateRefreshToken();

            // HASH the refresh token
            var (hash, salt) = TokenHasher.HashToken(rawRefreshToken);

            // Save HASHED token in DB
            var refreshTokenEntity = new RefreshToken
            {
                TokenHash = hash,
                TokenSalt = salt,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                UserId = user.Id
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            // Return RAW token to client
            return Ok(new
            {
                accessToken,
                refreshToken = rawRefreshToken
            });
        }

        // ================================
        // 🔄 REFRESH TOKEN (HASH VERIFIED)
        // ================================
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.RefreshToken))
                return BadRequest(new { message = "Invalid refresh request" });

            // ✅ validate via service (HASHED lookup)
            var storedToken = await _refreshTokenService
                .GetValidRefreshTokenAsync(request.RefreshToken);

            if (storedToken == null)
                return Unauthorized(new { message = "Invalid or expired refresh token" });

            var user = storedToken.User!;

            // ✅ revoke old token (rotation security)
            await _refreshTokenService.RevokeTokenAsync(request.RefreshToken);

            // ✅ generate new tokens
            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = _refreshTokenService.GenerateRefreshToken();

            await _refreshTokenService.SaveRefreshTokenAsync(user.Id, newRefreshToken);

            return Ok(new
            {
                accessToken = newAccessToken,
                refreshToken = newRefreshToken
            });
        }

        // ================================
        // 🚪 LOGOUT (HASH SAFE)
        // ================================
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest(new { message = "Refresh token is required" });

            await _refreshTokenService.RevokeTokenAsync(refreshToken);

            return Ok(new { message = "Logged out successfully" });
        }

        // ================================
        // JWT GENERATOR
        // ================================
        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role ?? "User")
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}