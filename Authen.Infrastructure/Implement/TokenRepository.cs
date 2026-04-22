using Authen.Application.Interface;
using Authen.Infrastructure.Constant;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Authen.Infrastructure.Implement
{
    public class TokenRepository : ITokenRepository
    {
        private readonly IOptions<JwtConfig> _jwtConfig;

        public TokenRepository(IOptions<JwtConfig> jwtConfig)
        {
            _jwtConfig = jwtConfig;
        }

        public string GenerateAccessToken(
            string userId,
            string userName,
            string email,
            IList<string> roles,
            int? customerProfileId = null,
            int? staffProfileId = null)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(email))
                throw new InvalidOperationException("UserName hoặc Email không hợp lệ để tạo access token.");

            if (string.IsNullOrWhiteSpace(_jwtConfig.Value.SecretKey))
                throw new InvalidOperationException("JWT SecretKey chưa được cấu hình.");

            var authClaims = new List<Claim>
            {
                new(ClaimTypes.Name, userName),
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Email, email),
                new(JwtRegisteredClaimNames.Sub, userId),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            if (customerProfileId.HasValue)
                authClaims.Add(new Claim("customer_id", customerProfileId.Value.ToString()));

            if (staffProfileId.HasValue)
                authClaims.Add(new Claim("staff_id", staffProfileId.Value.ToString()));

            foreach (var role in roles)
                authClaims.Add(new Claim(ClaimTypes.Role, role));

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Value.SecretKey));
            var token = new JwtSecurityToken(
                issuer: _jwtConfig.Value.Issuer,
                audience: _jwtConfig.Value.Audience,
                expires: DateTime.UtcNow.AddMinutes(_jwtConfig.Value.ExpireInMinutes),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public string HashRefreshToken(string refreshToken)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
            return Convert.ToHexString(bytes);
        }

        public int GetAccessTokenExpiryMinutes() => _jwtConfig.Value.ExpireInMinutes;
    }
}
