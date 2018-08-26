using Microsoft.AspNetCore.Mvc;
using Lucent.Common.Entities;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Telefrek.LDAP.Managers;
using Telefrek.LDAP;
using System;
using System.Text;
using System.Threading;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace Lucent.Portal.Models
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        ILDAPUserManager _manager;

        public LoginModel(ILDAPUserManager manager)
        {
            _manager = manager;
        }

        [BindProperty]
        public LoginEntity LoginEntity { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var principal = await _manager.TryAuthenticate(LoginEntity.Username, LoginEntity.Domain, LoginEntity.Credentials, CancellationToken.None);

                if (principal != null)
                {
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = LoginEntity.RememberMe });
                    return Page();
                }

                ModelState.AddModelError("", "credentials are invalid");
                return Page();
            }

            return Page();
        }
    }
}