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
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Lucent.Portal.Models
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        ILDAPUserManager _manager;
        ILogger<LoginModel> _log;

        public LoginModel(ILDAPUserManager manager, ILogger<LoginModel> log)
        {
            _manager = manager;
            _log = log;
        }

        [BindProperty]
        public LoginEntity LoginEntity { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                LoginEntity.Domain = HttpContext.Request.Host.Host;
                while (LoginEntity.Domain.Count(c => c == '.') > 1)
                    LoginEntity.Domain = LoginEntity.Domain.Substring(LoginEntity.Domain.IndexOf('.') + 1);

                var principal = await _manager.TryAuthenticate(LoginEntity.Username, LoginEntity.Domain, LoginEntity.Credentials, CancellationToken.None);

                if (principal != null)
                {
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = LoginEntity.RememberMe });
                        return RedirectToPage("Campaigns");
                }

                ModelState.AddModelError("", "credentials are invalid");
                return Page();
            }

            return Page();
        }
    }
}