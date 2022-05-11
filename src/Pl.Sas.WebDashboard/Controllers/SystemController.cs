﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Pl.Sas.WebDashboard.Models;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Infrastructure.Identity;
using System.Text.Json;

namespace Pl.Sas.WebDashboard.Controllers
{
    [Authorize(Roles = PermissionConstants.SystemManager)]
    public class SystemController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly UserService _userService;
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly IAsyncCacheService _asyncCacheService;
        private readonly ISystemData _systemData;

        public SystemController(
            ISystemData systemData,
            IAsyncCacheService asyncCacheService,
            IMemoryCacheService memoryCacheService,
            UserService userService,
            UserManager<User> userManager,
            ILogger<HomeController> logger)
        {
            _logger = logger;
            _userManager = userManager;
            _userService = userService;
            _memoryCacheService = memoryCacheService;
            _asyncCacheService = asyncCacheService;
            _systemData = systemData;
        }

        public IActionResult SystemLog()
        {
            return View();
        }

        public IActionResult Utilities()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UtilitiesAsync([FromBody] UtilitiesModel utilitiesModel)
        {
            var message = "Dữ liệu không hợp lệ.";
            var status = 0;

            if (utilitiesModel is not null)
            {
                _logger.LogWarning("Utilities active => : {json}", JsonSerializer.Serialize(utilitiesModel));
                switch (utilitiesModel.Type)
                {
                    case 1:
                        var updateResult = await _systemData.UtilityUpdateAsync(utilitiesModel.SchedulerType, utilitiesModel.Code);
                        message = updateResult ? "Kích hoạt lịch thành công." : "Kích hoạt lịch không thành công.";
                        status = updateResult ? 1 : 0;
                        break;

                    case 2:
                        var createUserResult = await _userService.CreateUser(utilitiesModel.Email, utilitiesModel.FullName, utilitiesModel.Password, utilitiesModel.Avatar);
                        status = createUserResult ? 1 : 0;
                        message = createUserResult ? "Thêm mới người dùng thành công." : "Thêm mới người dùng không thành công.";
                        break;

                    case 3:
                        var changeUser = await _userManager.FindByEmailAsync(utilitiesModel.EmailOrId);
                        if (changeUser is null)
                        {
                            changeUser = await _userManager.FindByIdAsync(utilitiesModel.EmailOrId);
                        }
                        if (changeUser is not null)
                        {
                            var removePasswordResult = await _userManager.RemovePasswordAsync(changeUser);
                            if (removePasswordResult.Succeeded)
                            {
                                var updatePasswordResult = await _userManager.AddPasswordAsync(changeUser, utilitiesModel.NewPassword);
                                status = updatePasswordResult.Succeeded ? 1 : 0;
                                message = updatePasswordResult.Succeeded ? "Đổi mật khẩu thành công." : "Đổi mật khẩu không thành công.";
                            }
                        }
                        break;

                    case 4:
                        await _asyncCacheService.ClearAsync();
                        _memoryCacheService.Clear();
                        status = 1;
                        message = "Xóa cache thành công.";
                        break;

                    case 5:
                        var deleteUser = await _userManager.FindByEmailAsync(utilitiesModel.Email);
                        if (deleteUser is null)
                        {
                            deleteUser = await _userManager.FindByIdAsync(utilitiesModel.Email);
                        }
                        if (deleteUser is not null)
                        {
                            var deleteResult = await _userManager.DeleteAsync(deleteUser);
                            status = deleteResult.Succeeded ? 1 : 0;
                            message = deleteResult.Succeeded ? "Xóa người dùng thành công." : "Xóa người dùng không thành công.";
                        }
                        break;

                    default:
                        break;
                }
            }

            return Json(new
            {
                status,
                message
            });
        }
    }
}