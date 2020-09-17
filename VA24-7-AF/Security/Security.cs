using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;

namespace VA24_7_AF.Security
{
    public static class Security
    {
        public static JwtSecurityToken GetClaims(HttpRequestMessage req)
        {
            string token = req.Headers.Authorization.ToString().Replace("Bearer ", "");
            var stream = token;
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(stream);
            var tokenClaims = handler.ReadToken(stream) as JwtSecurityToken;

            return tokenClaims;
        }
    }
}