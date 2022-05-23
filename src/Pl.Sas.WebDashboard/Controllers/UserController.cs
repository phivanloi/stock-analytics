using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Pl.Sas.Core.Entities.Security;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.WebDashboard.Models;
using System.Security.Claims;

namespace Pl.Sas.WebDashboard.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserData _userData;

        public UserController(IUserData userData)
        {
            _userData = userData;
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
                var user = await _userData.FindAsync(loginViewModel.Email);
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