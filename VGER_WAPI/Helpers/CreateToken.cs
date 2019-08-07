using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace VGER_WAPI.Helpers
{
    public class CreateToken
    {

        public static JwtSecurityToken GenerateToken(string UserName, string SecurityKey, string Issuer, string Audience, string CompanyName, string roles, int tokenTimeout)
        {
            //var claims = new[] { new Claim(ClaimTypes.Name, UserName) };
            var claims = new[] { new Claim(ClaimTypes.Name, UserName), new Claim(ClaimTypes.System, CompanyName), new Claim(ClaimTypes.Role, roles) };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecurityKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(tokenTimeout),
                signingCredentials: creds,
                notBefore: DateTime.Now);

            return token;
        }
    }
}
