using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.WebDashboard.Models;
using System.Text.Json;

namespace Pl.Sas.WebDashboard.Controllers
{
    [Authorize]
    public class SystemController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserData _userData;
        private readonly IMemoryCacheService _memoryCacheService;
        private readonly IAsyncCacheService _asyncCacheService;
        private readonly IScheduleData _scheduleData;

        public SystemController(
            IScheduleData scheduleData,
            IUserData userData,
            IAsyncCacheService asyncCacheService,
            IMemoryCacheService memoryCacheService,
            ILogger<HomeController> logger)
        {
            _scheduleData = scheduleData;
            _logger = logger;
            _memoryCacheService = memoryCacheService;
            _asyncCacheService = asyncCacheService;
            _userData = userData;
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
                        var updateResult = await _scheduleData.UtilityUpdateAsync(utilitiesModel.SchedulerType, utilitiesModel.Code);
                        message = updateResult ? "Kích hoạt lịch thành công." : "Kích hoạt lịch không thành công.";
                        status = updateResult ? 1 : 0;
                        break;

                    case 2:
                        var createUserResult = await _userData.CreateUser(utilitiesModel.Email, utilitiesModel.FullName, utilitiesModel.Password, utilitiesModel.Avatar);
                        status = createUserResult ? 1 : 0;
                        message = createUserResult ? "Thêm mới người dùng thành công." : "Thêm mới người dùng không thành công.";
                        break;

                    case 3:
                        var check = await _userData.SetPassowrdAsync(utilitiesModel.EmailOrId, utilitiesModel.NewPassword);
                        status = check ? 1 : 0;
                        message = check ? "Đổi mật khẩu thành công." : "Đổi mật khẩu không thành công.";
                        break;

                    case 4:
                        await _asyncCacheService.ClearAsync();
                        _memoryCacheService.Clear();
                        status = 1;
                        message = "Xóa cache thành công.";
                        break;

                    case 5:
                        var checkDelete = await _userData.DeleteAsync(utilitiesModel.EmailOrId);
                        status = checkDelete ? 1 : 0;
                        message = checkDelete ? "Xóa người dùng thành công." : "Xóa người dùng không thành công.";
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