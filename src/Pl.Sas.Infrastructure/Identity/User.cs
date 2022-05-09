using Microsoft.AspNetCore.Identity;

namespace Pl.Sas.Infrastructure.Identity
{
    public class User : IdentityUser
    {
        /// <summary>
        /// Ảnh đại diện
        /// </summary>
        public string? Avatar { get; set; }

        /// <summary>
        /// Tên đầy đủ
        /// </summary>
        public string FullName { get; set; } = null!;

        /// <summary>
        /// Trạng thái hoạt động
        /// </summary>
        public bool Active { get; set; } = false;

        /// <summary>
        /// Tài khoản đã bị xóa
        /// </summary>
        public bool Deleted { get; set; } = false;

        /// <summary>
        /// Ngày tạo
        /// </summary>
        public DateTime CreatedTime { get; set; } = DateTime.Now;

        /// <summary>
        /// Thời điểm xem thông báo cuối cùng
        /// </summary>
        public DateTime LastNotificationViewTime { get; set; } = DateTime.Now;
    }
}