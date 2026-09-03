using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace __RootNamespace__.Common.Auth
{
    public class JwtHandler
    {
        private readonly string secretKey;
        private readonly string issuer;
        private readonly string audience;
        private readonly int expires;
        private readonly IConfiguration _configuration;

        public JwtHandler(IConfiguration configuration)
        {
            secretKey = configuration.GetValue<string>("Jwt:SecretKey")!;
            issuer = configuration.GetValue<string>("Jwt:Issuer")!;
            audience = configuration.GetValue<string>("Jwt:Audience")!;
            expires = configuration.GetValue<int>("Jwt:Expires");
            _configuration = configuration;
        }

        public string GenerateToken(string username, IDictionary<string, object> userData)
        {
            var exp_token = DateTime.UtcNow.AddMinutes(expires);

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.UserData, JsonConvert.SerializeObject(userData)),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: exp_token,
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public static JwtSecurityToken ReadToken(string jwtToken)
        {
            var arr = jwtToken.Split(' ');
            if (arr.Length == 2)
            {
                jwtToken = arr[1];
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(jwtToken);

            return token;
        }
    }
}
