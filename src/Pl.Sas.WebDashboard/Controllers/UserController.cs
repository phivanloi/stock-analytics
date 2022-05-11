using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Pl.Sas.WebDashboard.Models;
using Pl.Sas.Infrastructure.Identity;

namespace Pl.Sas.WebDashboard.Controllers
{
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public UserController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<UserController> logger)
        {
            _userManager = userManager;
            _logger = logger;
            _signInManager = signInManager;
        }

        [HttpGet("/dang-nhap")]
        public async Task<IActionResult> LoginAsync(string? returnUrl = null)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("/dang-nhap")]
        public async Task<IActionResult> LoginAsync(LoginViewModel loginViewModel, string? returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(loginViewModel.Email);
                if (user is null)
                {
                    user = await _userManager.FindByEmailAsync(loginViewModel.Email);
                }

                if (user is not null && !user.Deleted)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, loginViewModel.Password, loginViewModel.RememberMe, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        _logger.LogWarning("User {Email} logged in at {Now}.", user.Email, DateTime.Now);
                        returnUrl ??= "/";
                        return LocalRedirect(returnUrl);
                    }
                    if (result.RequiresTwoFactor)
                    {
                        return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, loginViewModel.RememberMe });
                    }
                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning("User account locked out.");
                        return RedirectToPage("./Lockout");
                    }
                }
                ModelState.AddModelError(string.Empty, "Đăng nhập không thành công.");
            }
            return View(loginViewModel);
        }

        [HttpGet("/dang-xuat")]
        public async Task<IActionResult> LogoutAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return LocalRedirect(returnUrl);
        }
    }
}