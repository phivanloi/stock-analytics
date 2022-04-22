namespace Pl.Sas.Core.Entities
{
    /// <summary>
    /// Ngành nghề
    /// </summary>
    public class Industry : BaseEntity
    {
        /// <summary>
        /// Tên loại hình công ty
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Mã ngành bên ssi crawler về
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Thứ tự hiển thị
        /// </summary>
        public int Ordinal { get; set; } = 1;

        /// <summary>
        /// Trạng thái hoạt động
        /// </summary>
        public bool Activated { get; set; } = true;

        /// <summary>
        /// Đánh giá khả năng tăng trưởng của ngành trong giai đoạn hiện tại
        /// giúp hệ thống có thể tăng hoặc giảm chỉ số đầu tư và chỉ số thu hồi vốn của một mã cổ phiếu.
        /// Rank này được nhập vào do con người đánh giá
        /// </summary>
        public int Rank { get; set; } = 0;

        /// <summary>
        /// Điểm đánh giá tăng trường theo hành động giá
        /// </summary>
        public int AutoRank { get; set; } = 0;
    }
}