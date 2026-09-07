using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthenticationDeepDive.Weather.IdentityProvider.Controllers
{
    [ApiController]
    [Route("auth")]
    public class TokenController : ControllerBase
    {
        private readonly ILogger<TokenController> _logger;

        public TokenController(ILogger<TokenController> logger)
        {
            _logger = logger;
        }

        [HttpGet("getToken")]
        public AuthToken GenerateToken()
        {
            var expirationTime = DateTime.UtcNow.AddHours(1); // Token valid for 1 hour
            Claim[] claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, "dummy-user"),
                new Claim(ClaimTypes.Role, "User"),
                new Claim("userId", "abc123"),
                new Claim("groups", "w-read,w-readwrite,l-read")
            };
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("your-256-bit-secretyour-256-bit-secretyour-256-bit-secret"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: "https://localhost:5001",
                audience: "https://localhost:5001",
                claims: claims,
                expires: expirationTime,
                signingCredentials: creds
            );
            var tokenstring = new JwtSecurityTokenHandler().WriteToken(token);
            var response = new AuthToken
            {
                Token = tokenstring,
                ExpiresIn = 1
            };
            return response;
        }

        [HttpGet("validateToken")]
        public bool ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = System.Text.Encoding.UTF8.GetBytes("your-256-bit-secretyour-256-bit-secretyour-256-bit-secret");
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = "https://localhost:5001",
                    ValidateAudience = true,
                    ValidAudience = "https://localhost:5001",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
