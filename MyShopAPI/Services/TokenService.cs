using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyShopAPI.Data;
using MyShopAPI.Models;

namespace MyShopAPI.Services
{
    /// <summary>
    /// Implementation of ITokenService for JWT access token and refresh token management.
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public TokenService(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        /// <inheritdoc />
        public Task<(string Token, DateTime ExpiresAt)> GenerateAccessTokenAsync(ApplicationUser user, string role)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
            var issuer = jwtSettings["Issuer"] ?? "MyShopAPI";
            var audience = jwtSettings["Audience"] ?? "MyShopApp";
            var expiryMinutes = int.Parse(jwtSettings["AccessTokenExpiryMinutes"] ?? "60");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.ShopName),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return Task.FromResult((tokenString, expiresAt));
        }

        /// <inheritdoc />
        public async Task<RefreshToken> GenerateRefreshTokenAsync(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var expiryDays = int.Parse(jwtSettings["RefreshTokenExpiryDays"] ?? "7");

            // Generate a secure random token
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var tokenString = Convert.ToBase64String(randomBytes);

            var refreshToken = new RefreshToken
            {
                Token = tokenString,
                ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
                CreatedAt = DateTime.UtcNow,
                UserId = user.Id
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return refreshToken;
        }

        /// <inheritdoc />
        public async Task<ApplicationUser?> ValidateRefreshTokenAsync(string token)
        {
            // Need to bypass query filters to find the token
            var refreshToken = await _context.RefreshTokens
                .IgnoreQueryFilters()
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == token);

            if (refreshToken == null || !refreshToken.IsActive)
            {
                return null;
            }

            return refreshToken.User;
        }

        /// <inheritdoc />
        public async Task RevokeAllUserTokensAsync(string userId)
        {
            var tokens = await _context.RefreshTokens
                .IgnoreQueryFilters()
                .Where(r => r.UserId == userId && !r.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }

            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task RevokeTokenAsync(string token)
        {
            var refreshToken = await _context.RefreshTokens
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.Token == token);

            if (refreshToken != null)
            {
                refreshToken.IsRevoked = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
