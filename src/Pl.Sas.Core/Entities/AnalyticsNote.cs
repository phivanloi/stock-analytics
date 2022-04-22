namespace Pl.Sas.Core.Entities
{
    /// <summary>
    /// Ghi chú các luồng phân tích trong hệ thống
    /// </summary>
    public class AnalyticsNote
    {
        /// <summary>
        /// Nội dung thông báo, diễn giải
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Loại thông báo
        /// <para>-1 cảnh báo</para>
        /// <para>0 bình thường</para>
        /// <para>1 là tốt</para>
        /// </summary>
        public int Type { get; set; } = 0;

        /// <summary>
        /// Đường link tìm hiểu về phân tích này
        /// </summary>
        public string? GuideLink { get; set; }

        /// <summary>
        /// kết quả đánh giá
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Khởi tạo
        /// </summary>
        /// <param name="message">Nội dung thông báo</param>
        /// <param name="score">Điểm đánh giá</param>
        /// <param name="type">Loại thông báo</param>
        /// <param name="guideLink">Đường link bài viết phân tích về chỉ số đó</param>
        public AnalyticsNote(string message, int score, int type, string? guideLink = null) => (Message, Score, Type, GuideLink) = (message, score, type, guideLink);
    }
}