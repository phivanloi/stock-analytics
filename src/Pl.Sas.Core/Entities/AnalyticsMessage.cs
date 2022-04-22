namespace Pl.Sas.Core.Entities
{
    public class AnalyticsMessage
    {
        /// <summary>
        /// Nội dung thông báo, diễn giải
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Loại thông báo
        /// <para>-1 cảnh báo</para>
        /// <para>0 là thông tin</para>
        /// <para>1 là tốt</para>
        /// </summary>
        public int Type { get; set; } = 0;

        /// <summary>
        /// đường dẫn tìm hiểu
        /// </summary>
        public string? GuideLink { get; set; }

        /// <summary>
        /// kết quả đánh giá
        /// </summary>
        public int Score { get; set; }

        public AnalyticsMessage(string message, int score, int type, string? guideLink = null) => (Message, Score, Type, GuideLink) = (message, score, type, guideLink);
    }
}