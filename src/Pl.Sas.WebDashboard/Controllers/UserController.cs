using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pl.Sas.WebDashboard.Models;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Infrastructure;
using Pl.Sas.Infrastructure.Identity;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Pl.Sas.WebDashboard.Controllers
{
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IUserNotificationData _userNotificationData;
        private readonly IZipHelper _zipHelper;

        public UserController(
            IZipHelper zipHelper,
            IUserNotificationData userNotificationData,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<UserController> logger)
        {
            _userManager = userManager;
            _logger = logger;
            _signInManager = signInManager;
            _userNotificationData = userNotificationData;
            _zipHelper = zipHelper;
        }

        [HttpGet("/dang-nhap")]
        public async Task<IActionResult> LoginAsync(string returnUrl = null)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("/dang-nhap")]
        public async Task<IActionResult> LoginAsync(LoginViewModel loginViewModel, string returnUrl = null)
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
                        _logger.LogWarning($"User {user.Email} logged in at {DateTime.Now}.");
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
        public async Task<IActionResult> LogoutAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return LocalRedirect(returnUrl);
        }

        [Authorize(Roles = PermissionConstants.CmsDashbroad)]
        public async Task<IActionResult> NotificationsAsync()
        {
            var model = new NotificationViewModel();
            var userId = HttpContext.GetUserId();
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return View(model);
            }
            user.LastNotificationViewTime = DateTime.Now;
            var updateTask = _userManager.UpdateAsync(user);
            var userNotifications = await _userNotificationData.FindAllAsync(userId);
            foreach (var userNotification in userNotifications)
            {
                model.Notifications.Add(new UserNotificationViewModel()
                {
                    Id = userNotification.Id,
                    Symbol = userNotification.Symbol,
                    Title = userNotification.Title,
                    CreatedTime = userNotification.CreatedTime,
                    Content = Encoding.UTF8.GetString(_zipHelper.UnZipByte(userNotification.ZipMessage))
                });
            }
            await updateTask;
            return View(model);
        }

        [Authorize(Roles = PermissionConstants.CmsDashbroad)]
        public async Task<IActionResult> NewNotificationsCountAsync()
        {
            var userId = HttpContext.GetUserId();
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return Content("0");
            }
            var count = await _userNotificationData.GetNewUserNotificationCountAsync(userId, user.LastNotificationViewTime);
            return Content(count.ToString());
        }

        [Authorize(Roles = PermissionConstants.CmsDashbroad)]
        public async Task<IActionResult> DeleteNotificationsAsync(string id)
        {
            var checkDeleteNotification = await _userNotificationData.DeleteAsync(id);
            return Json(checkDeleteNotification ? 1 : 0);
        }
    }
}