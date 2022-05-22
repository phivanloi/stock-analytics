namespace Pl.Sas.Core.Entities
{
    public class User : BaseEntity
    {
        /// <summary>
        /// Tên tài khoản dùng để đăng nhập hiện đang sử dụng email
        /// hoangvana@gmail.com
        /// </summary>
        public string UserName { get; set; } = null!;

        /// <summary>
        /// Mật khẩu đăng nhập
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Tài khoản email
        /// hoangvana@gmail.com
        /// </summary>
        public string? Email { get; set; } = null;

        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string? Phone { get; set; } = null;

        /// <summary>
        /// Ngày tháng năm sinh
        /// </summary>
        public DateTime? DateOfBirth { get; set; } = null;

        /// <summary>
        /// Ảnh đại diện
        /// </summary>
        public string? Avatar { get; set; }

        /// <summary>
        /// Tên đầy đủ
        /// </summary>
        public string FullName { get; set; } = null!;

        /// <summary>
        /// Trạng thái tài khoản quản trị
        /// </summary>
        public bool IsAdministator { get; set; } = false;

        /// <summary>
        /// Trạng thái hoạt động
        /// </summary>
        public bool Active { get; set; } = false;

        /// <summary>
        /// Tài khoản đã bị xóa
        /// </summary>
        public bool Deleted { get; set; } = false;
    }
}