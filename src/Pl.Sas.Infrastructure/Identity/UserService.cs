using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Pl.Sas.Core;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Entities.Security;
using Pl.Sas.Core.Interfaces;
using System.Security.Claims;

namespace Pl.Sas.Infrastructure.Identity
{
    public class UserService
    {
        private readonly IdentityDbContext _identityDbContext;
        private readonly IMemoryCacheService _memoryCacheService;

        public UserService(
           IdentityDbContext identityDbContext,
            IMemoryCacheService memoryCacheService)
        {
            _identityDbContext = identityDbContext;
            _memoryCacheService = memoryCacheService;
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
                Deleted = false,
                FullName = fullName,
                Avatar = avatar,
                Password = Cryptography.CreateMd5Password(password)
            };
            _identityDbContext.Users.Add(user);
            return await _identityDbContext.SaveChangesAsync() > 0;
        }

        public virtual async Task<bool> SetPassowrdAsync(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return false;
            }

            var user = await _identityDbContext.Users.FirstOrDefaultAsync(q => q.UserName == email || q.Id == email);
            if (user is not null)
            {
                user.Password = Cryptography.CreateMd5Password(password);
                return await _identityDbContext.SaveChangesAsync() > 0;
            }
            return false;
        }

        public virtual async Task<bool> DeleteAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return false;
            }

            var user = await _identityDbContext.Users.FirstOrDefaultAsync(q => q.UserName == email || q.Id == email);
            if (user is not null)
            {
                _identityDbContext.Users.Remove(user);
                return await _identityDbContext.SaveChangesAsync() > 0;
            }
            return false;
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
                return await _identityDbContext.Users.AsNoTracking().FirstOrDefaultAsync(q => q.Id == userId);
            }, Constants.DefaultCacheTime * 60 * 24);
        }
    }
}