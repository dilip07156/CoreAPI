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
using VGER_WAPI_CLASSES;
using Microsoft.AspNetCore.Http;

namespace VGER_DISTRIBUTION.Helpers
{
    public class CreateToken
    {

        public static JwtSecurityToken GenerateToken(string UserName, string SecurityKey, string Issuer, string Audience, string CompanyName, string roles)
        {
            var claims = new[] { new Claim(ClaimTypes.Name, UserName), new Claim(ClaimTypes.System, CompanyName), new Claim(ClaimTypes.Role, roles)};
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecurityKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: creds,
                notBefore: DateTime.Now);

            return token;
        }

        public static UserCookieDetail ReadToken(HttpContext httpContext)
        {
            var res = new UserCookieDetail();
            var claims = ((System.Security.Claims.ClaimsIdentity)httpContext.User.Identity).Claims.ToList();

            foreach (Claim curClaim in claims)
            {
                if(curClaim.Type.ToLower().Contains("name"))
                    res.UserName = curClaim.Value;
                if (curClaim.Type.ToLower().Contains("role"))
                    res.RoleName = curClaim.Value;
                if (curClaim.Type.ToLower().Contains("system"))
                    res.CompanyName = curClaim.Value;
                //if (curClaim.Type.ToLower().Contains("actor"))
                //{
                //    if(curClaim.Value.ToLower().Contains("S"))
                //        res.IsSupplier = true;
                //    if (curClaim.Value.ToLower().Contains("A"))
                //        res.IsAgent = true;
                //}
            }

            return res;
        }
    }
}
