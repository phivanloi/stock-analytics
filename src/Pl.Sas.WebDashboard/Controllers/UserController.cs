using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Pl.Sas.WebDashboard.Models;
using Pl.Sas.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Pl.Sas.Core.Entities.Security;

namespace Pl.Sas.WebDashboard.Controllers
{
    public class UserController : Controller
    {
        private readonly IdentityDbContext _identityDbContext;

        public UserController(
            IdentityDbContext identityDbContext)
        {
            _identityDbContext = identityDbContext;
        }

        [HttpGet("/dang-nhap")]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("/dang-nhap")]
        public async Task<IActionResult> LoginAsync(LoginViewModel loginViewModel, string? returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var user = await _identityDbContext.Users.FirstOrDefaultAsync(q => q.UserName == loginViewModel.Email);
                if (user is not null && user.Active && !user.Deleted && user.Password == Cryptography.CreateMd5Password(loginViewModel.Password))
                {
                    var identitys = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                    identitys.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
                    identitys.AddClaim(new Claim(ClaimTypes.IsPersistent, loginViewModel.RememberMe ? "1" : "0"));
                    ClaimsPrincipal userPrincipal = new(identitys);
                    AuthenticationProperties authenticationProperties = new()
                    {
                        IsPersistent = loginViewModel.RememberMe,
                        IssuedUtc = DateTime.UtcNow,
                        AllowRefresh = true,
                    };
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal, authenticationProperties);
                    returnUrl ??= "/";
                    return LocalRedirect(returnUrl);
                }
                ModelState.AddModelError(string.Empty, "Đăng nhập không thành công.");
            }
            return View(loginViewModel);
        }

        [HttpGet("/dang-xuat")]
        public async Task<IActionResult> LogoutAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            await HttpContext.SignOutAsync();
            return LocalRedirect(returnUrl);
        }
    }
}