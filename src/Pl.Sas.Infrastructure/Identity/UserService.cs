using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pl.Sas.Core;
using Pl.Sas.Core.Interfaces;
using System.Security.Claims;

namespace Pl.Sas.Infrastructure.Identity
{
    public class UserService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMemoryCacheService _memoryCacheService;

        public UserService(
            RoleManager<IdentityRole> roleManager,
            IMemoryCacheService memoryCacheService,
            UserManager<User> userManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _memoryCacheService = memoryCacheService;
        }

        /// <summary>
        /// Kiểm tra xem thông tin đăng nhập hiện tại có quyền truy cập vào một role hay không
        /// </summary>
        /// <param name="claimsPrincipal">Thông tin đăng nhập</param>
        /// <param name="role">Quyền</param>
        /// <returns>bool</returns>
        public virtual async Task<bool> UserIsInRoleAsync(ClaimsPrincipal claimsPrincipal, string role)
        {
            if (claimsPrincipal is null || string.IsNullOrEmpty(role))
            {
                return false;
            }
            var user = await _userManager.GetUserAsync(claimsPrincipal);
            if (user is null)
            {
                return false;
            }
            return await _userManager.IsInRoleAsync(user, role);
        }

        /// <summary>
        /// Kiểm tra xem người dùng hiện tạ có quyền không
        /// </summary>
        /// <param name="user">User cần kiểm tra</param>
        /// <param name="role">Quyền</param>
        /// <returns>Task bool</returns>
        public virtual async Task<bool> UserIsInRoleAsync(User user, string role)
        {
            if (user is null || string.IsNullOrEmpty(role))
            {
                return false;
            }
            return await _userManager.IsInRoleAsync(user, role);
        }

        /// <summary>
        /// Create default user for test
        /// </summary>
        /// <returns>Task</returns>
        public virtual async Task<string> CreateDefaultUserAsync()
        {
            if (_userManager.Users.Any())
            {
                return string.Empty;
            }

            User user = new()
            {
                UserName = "phivanloi@gmail.com",
                Email = "phivanloi@gmail.com",
                Active = true,
                EmailConfirmed = true,
                Deleted = false,
                FullName = "Quản trị viên",
                Avatar = "http://s120-ava-talk.zadn.vn/a/4/2/3/1/120/e874de0ca7c1ac55ddb4c1e047e463d8.jpg",
                PhoneNumber = "0906282026"
            };

            await AddRoleToSystemAsync(PermissionConstants.AdministratorRoles);
            var createUserResult = await _userManager.CreateAsync(user, "liemtinmoi2413");
            if (createUserResult.Succeeded)
            {
                await _userManager.AddToRolesAsync(user, new List<string>() { PermissionConstants.CmsDashbroad, PermissionConstants.SystemManager });
            }
            return user.Id;

            async Task AddRoleToSystemAsync(IEnumerable<Permission> permissions)
            {
                foreach (var permission in permissions)
                {
                    if (!await _roleManager.RoleExistsAsync(permission.Role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(permission.Role));
                    }
                    await AddRoleToSystemAsync(permission.Permissions);
                }
            }
        }

        public virtual async Task<bool> CreateUser(string email, string fullName, string password, string avatar = "")
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(password))
            {
                return false;
            }

            User user = new()
            {
                UserName = email,
                Email = email,
                Active = true,
                EmailConfirmed = true,
                Deleted = false,
                FullName = fullName,
                Avatar = avatar
            };
            var createUserResult = await _userManager.CreateAsync(user, password);
            await _userManager.AddToRolesAsync(user, new List<string>() { PermissionConstants.CmsDashbroad });
            return createUserResult.Succeeded;
        }

        /// <summary>
        /// Get user info from cache
        /// </summary>
        /// <param name="user">Claims principal of logged user</param>
        /// <returns>User</returns>
        public virtual async Task<User?> CacheGetUserInfoAsync(ClaimsPrincipal user)
        {
            Guard.Against.Null(user, nameof(user));
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return await CacheGetUserInfoAsync(userId);
        }

        /// <summary>
        /// Get user info from cache
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>User</returns>
        public virtual async Task<User?> CacheGetUserInfoAsync(string userId)
        {
            Guard.Against.NullOrEmpty(userId, nameof(userId));
            var cacheKey = $"cgui-usi{userId}";
            return await _memoryCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                return await _userManager.Users.AsNoTracking().FirstOrDefaultAsync(q => q.Id == userId);
            }, Constants.DefaultCacheTime * 60 * 24);
        }
    }
}