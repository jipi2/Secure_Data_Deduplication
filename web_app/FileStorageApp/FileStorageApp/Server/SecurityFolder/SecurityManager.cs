using FileStorageApp.Server.Entity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace FileStorageApp.Server.SecurityFolder
{
    public class SecurityManager 
    {
        private readonly IConfiguration _configuration;
        public SecurityManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string GetNewJwt(User user)
        {

            var secretKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("Secret").Value));
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Email), // NOTE: this will be the "User.Identity.Name" value to retrieve the user name on the server side
                new Claim(ClaimTypes.Role, user.Roles[0].RoleName),
                new Claim("UserID", user.Id.ToString())
            };

            var token = new JwtSecurityToken(issuer: "fileStorage.com", audience: "fileStorage.com", claims: claims, expires: DateTime.Now.AddMonths(1), signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GetUserIdFromJWT(string jwt)
        {
            IEnumerable<Claim> claims = GetTokenClaims(jwt);
            return claims.Where(c => c.Type.Equals("UserID")).FirstOrDefault().Value;
        }
        public string GetUserEmailFromJwt(string jwt)
        {
            IEnumerable<Claim> claims = GetTokenClaims(jwt);
            return claims.Where(c => c.Type.Equals(ClaimTypes.Name)).FirstOrDefault().Value;
        }
        public IEnumerable<Claim> GetTokenClaims(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
