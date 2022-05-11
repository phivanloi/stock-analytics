namespace Pl.Sas.WebDashboard.Models
{
    public class UtilitiesModel
    {
        /// <summary>
        /// Loại utilities
        /// 1 là kích hoạt lịch
        /// 2 Thêm mới người dùng
        /// 3 là đổi mật khẩu cho người dùng
        /// </summary>
        public int Type { get; set; }

        #region Type = 1

        /// <summary>
        /// Mã cố phiếu
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Id loại lịch trong trường hợp kích hoạt lịch
        /// </summary>
        public int SchedulerType { get; set; }

        #endregion Type = 1

        #region Type = 3

        /// <summary>
        /// Email hoặc id người dùng
        /// </summary>
        public string EmailOrId { get; set; }

        /// <summary>
        /// Mật khẩu muốn đổi
        /// </summary>
        public string NewPassword { get; set; }

        #endregion Type = 3

        #region Type = 2

        /// <summary>
        /// Tên đầy đủ trong trường hợp thêm mới người dùng
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Avatar của người dùng trong trường hợp thêm mới người dùng
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// Mật khẩu
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Mật khẩu
        /// </summary>
        public string Email { get; set; }

        #endregion Type = 2
    }
}