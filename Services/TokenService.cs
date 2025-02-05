using Blog.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Blog.Services
{
    public class TokenService
    {
        public string GenerateToken(User user)
        {
            var key = Encoding.ASCII.GetBytes(Configuration.JwtKey);

            var tokenHandler = new JwtSecurityTokenHandler();
            // Configuração do token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddHours(8),

                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)

            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
