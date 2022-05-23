using Ardalis.GuardClauses;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Pl.Sas.Core;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Entities.Security;
using Pl.Sas.Core.Interfaces;
using System.Security.Claims;

namespace Pl.Sas.Infrastructure.Data
{
    public class UserData : BaseData, IUserData
    {
        private readonly IMemoryCacheService _memoryCacheService;

        public UserData(
            IMemoryCacheService memoryCacheService,
            IOptionsMonitor<ConnectionStrings> options) : base(options)
        {
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
                IsAdministator = false,
                Password = Cryptography.CreateMd5Password(password)
            };
            return await InsertAsync(user);
        }

        public virtual async Task<bool> InsertAsync(User user)
        {
            var query = $@"   INSERT INTO Users
                                        (Id,
                                        UserName,
                                        Password,
                                        Email,
                                        Phone,
                                        Sector,
                                        DateOfBirth,
                                        Avatar,
                                        FullName,
                                        IsAdministator,
                                        Active,
                                        Deleted,
                                        CreatedTime,
                                        UpdatedTime)
                                    VALUES
                                        (@Id,
                                        @UserName,
                                        @Password,
                                        @Email,
                                        @Phone,
                                        @Sector,
                                        @DateOfBirth,
                                        @Avatar,
                                        @FullName,
                                        @IsAdministator,
                                        @Active,
                                        @Deleted,
                                        @CreatedTime,
                                        @UpdatedTime)";
            using SqlConnection connection = new(_connectionStrings.IdentityConnection);
            return await connection.ExecuteAsync(query, user) > 0;
        }

        public virtual async Task<User> FindAsync(string userName)
        {
            Guard.Against.NullOrEmpty(userName, nameof(userName));
            var query = "SELECT * FROM Users WHERE UserName = @userName OR Email = @userName";
            using SqlConnection connection = new(_connectionStrings.IdentityConnection);
            return await connection.QueryFirstOrDefaultAsync<User>(query, new { userName });
        }

        public virtual async Task<bool> SetPassowrdAsync(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return false;
            }
            var md5Password = Cryptography.CreateMd5Password(password);
            var query = "UPDATE Users SET Password = @md5Password WHERE @email OR Email = @email";
            using SqlConnection connection = new(_connectionStrings.IdentityConnection);
            return await connection.ExecuteAsync(query, new { md5Password, email }) > 0;
        }

        public virtual async Task<bool> DeleteAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return false;
            }

            var query = "DELETE Users WHERE UserName = @email OR Email = @email";
            using SqlConnection connection = new(_connectionStrings.IdentityConnection);
            return await connection.ExecuteAsync(query, new { email }) > 0;
        }

        public virtual async Task<User?> CacheGetUserInfoAsync(ClaimsPrincipal user)
        {
            Guard.Against.Null(user, nameof(user));
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return await CacheGetUserInfoAsync(userId);
        }

        public virtual async Task<User?> CacheGetUserInfoAsync(string userId)
        {
            Guard.Against.NullOrEmpty(userId, nameof(userId));
            var cacheKey = $"cgui-usi{userId}";
            return await _memoryCacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                var query = "SELECT * FROM Users WHERE Id = @userId";
                using SqlConnection connection = new(_connectionStrings.IdentityConnection);
                return await connection.QueryFirstOrDefaultAsync<User>(query, new { userId });
            }, Constants.DefaultCacheTime * 60 * 24);
        }
    }
}