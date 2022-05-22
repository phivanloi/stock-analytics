using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    public interface IUserData
    {
        /// <summary>
        /// Tạo người dùng
        /// </summary>
        /// <param name="email">Email</param>
        /// <param name="fullName">Tên đầy đủ</param>
        /// <param name="password">Mật khẩu</param>
        /// <param name="avatar">Ảnh đại diện</param>
        /// <returns>bool</returns>
        Task<bool> CreateUser(string email, string fullName, string password, string avatar = "");

        /// <summary>
        /// Thêm mới người dùng
        /// </summary>
        /// <param name="user">Thông tin người dùng cần thêm</param>
        /// <returns>bool</returns>
        Task<bool> InsertAsync(User user);

        /// <summary>
        /// Tìm người dùng
        /// </summary>
        /// <param name="userName">Tài khoản cần tìm</param>
        /// <returns>User</returns>
        Task<User> FindAsync(string userName);

        /// <summary>
        /// Đặt lại mật khẩu
        /// </summary>
        /// <param name="email">Email cần đặt</param>
        /// <param name="password">Mật khẩu</param>
        /// <returns>bool</returns>
        Task<bool> SetPassowrdAsync(string email, string password);

        /// <summary>
        /// Xóa người dùng
        /// </summary>
        /// <param name="email">Email người dùng cần xóa</param>
        /// <returns>bool</returns>
        Task<bool> DeleteAsync(string email);

        /// <summary>
        /// Lấy thông tin người dùng từ cache
        /// </summary>
        /// <param name="userId">Id người dùng</param>
        /// <returns>User</returns>
        Task<User?> CacheGetUserInfoAsync(string userId);
    }
}