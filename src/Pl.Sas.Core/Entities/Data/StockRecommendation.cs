namespace Pl.Sas.Core.Entities
{
    /// <summary>
    /// Lưu những báo cáo của những công ty chứng khoán
    /// </summary>
    public class StockRecommendation : BaseEntity
    {
        /// <summary>
        /// Mã cổ phiếu
        /// </summary>
        public string Symbol { get; set; } = null!;

        /// <summary>
        /// Đơn vị ra báo cáo đánh giá
        /// </summary>
        public string Firm { get; set; } = null!;

        /// <summary>
        /// Loại kiến nghị, BUY, SELL, HOLD
        /// </summary>
        public string Type { get; set; } = null!;

        /// <summary>
        /// ngày báo cáo
        /// </summary>
        public DateTime ReportDate { get; set; }

        /// <summary>
        /// Nguồn báo cáo
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Báo cáo viên
        /// </summary>
        public string? Analyst { get; set; }

        /// <summary>
        /// Giá lúc làm báo cáo
        /// </summary>
        public float ReportPrice { get; set; }

        /// <summary>
        /// Giá mục tiêu
        /// </summary>
        public float TargetPrice { get; set; }

        /// <summary>
        /// Giá mục tiêu trung bình
        /// </summary>
        public float AvgTargetPrice { get; set; }
    }
}