namespace Pl.Sas.Core.Entities
{
    public class UserNotification : BaseEntity
    {
        /// <summary>
        /// Id người dùng
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Mã chứng khoán
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Nội dung thông báo
        /// </summary>
        public byte[] ZipMessage { get; set; } = null;

        /// <summary>
        /// Trạng thái đã đọc
        /// </summary>
        public bool Readed { get; set; } = false;

        /// <summary>
        /// Tiêu đề thông báo
        /// </summary>
        public string Title { get; set; }
    }
}