using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Lucent.Common
{
    /// <summary>
    /// Generate JWT tokens
    /// </summary>
    public class JwtTokenGenerator
    {
        /// <summary>
        /// Get a new bearer token
        /// </summary>
        /// <param name="expiration">The token expiration</param>
        /// <returns>A new signed token</returns>
        public string GetBearer(DateTime expiration)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("please don't use this"));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var tokeOptions = new JwtSecurityToken(
                issuer: "https://lucentbid.com",
                audience: "https://lucentbid.com",
                claims: new List<Claim>(),
                expires: expiration,
                signingCredentials: signinCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(tokeOptions);
        }
    }
}