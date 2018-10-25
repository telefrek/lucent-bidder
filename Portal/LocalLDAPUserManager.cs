using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Telefrek.LDAP.Managers;

namespace Lucent.Portal
{
    public class LocalLDAPUserManager : ILDAPUserManager
    {
        public Task<LDAPUser> FindUserAsync(string name, string domain, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }

        public Task<ClaimsPrincipal> TryAuthenticate(string name, string domain, string credentials, CancellationToken token)
        {
            // Create the identity from the user info
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, name));
            identity.AddClaim(new Claim(ClaimTypes.Name, name));

            // Return the initialized principal
            return Task.FromResult(new ClaimsPrincipal(identity));
        }
    }
}