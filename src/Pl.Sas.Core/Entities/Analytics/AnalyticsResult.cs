namespace Pl.Sas.Core.Entities
{
    public class AnalyticsResult : BaseEntity
    {
        public AnalyticsResult() { }

        public AnalyticsResult(string symbol, DateTime lastTradingDate)
        {
            Symbol = symbol;
            TradingDate = lastTradingDate;
        }

        /// <summary>
        /// Mã chứng khoán <see cref="Stock"/>
        /// </summary>
        public string Symbol { get; set; } = null!;

        /// <summary>
        /// Ngày phân tích, mỗi ngày giao dịch sẽ có một phân tích
        /// </summary>
        public DateTime TradingDate { get; set; }

        /// <summary>
        /// Điểm đánh giá tâm lý thị trường, dòng tiền
        /// </summary>
        public int MarketScore { get; set; } = -100;

        /// <summary>
        /// Ghi chú đánh giá thị trường là danh sách <see cref="AnalyticsNote"/>
        /// </summary>
        public byte[]? MarketNotes { get; set; } = null;

        /// <summary>
        /// Điểm đánh giá giá trị của doanh nghiệp
        /// </summary>
        public int CompanyValueScore { get; set; } = -100;

        /// <summary>
        /// Ghi chú đánh giá giá trị của doanh nghiệp <see cref="AnalyticsNote"/>
        /// </summary>
        public byte[]? CompanyValueNotes { get; set; } = null;

        /// <summary>
        /// Điểm đánh giá tăng trưởng doanh nghiệp
        /// </summary>
        public int CompanyGrowthScore { get; set; } = -100;

        /// <summary>
        /// Ghi chú đánh giá tăng trưởng của doanh nghiệp <see cref="AnalyticsNote"/>
        /// </summary>
        public byte[]? CompanyGrowthNotes { get; set; } = null;

        /// <summary>
        /// Điểm đánh giá kỹ thuật
        /// </summary>
        public int StockScore { get; set; } = -100;

        /// <summary>
        /// Ghi chú đánh giá giao dịch của chứng khoán <see cref="AnalyticsNote"/>
        /// </summary>
        public byte[]? StockNotes { get; set; } = null;

        /// <summary>
        /// Điểm đánh giá của fiin
        /// </summary>
        public int FiinScore { get; set; } = -100;

        /// <summary>
        /// Nội dung đánh giá của fiin <see cref="AnalyticsNote"/>
        /// </summary>
        public byte[]? FiinNotes { get; set; } = null;

        /// <summary>
        /// Điểm đánh giá của vnd
        /// </summary>
        public int VndScore { get; set; } = -100;

        /// <summary>
        /// Nội dung đánh giá của vnd <see cref="AnalyticsNote"/>
        /// </summary>
        public byte[]? VndNote { get; set; } = null;

        /// <summary>
        /// Giá dự phóng trung bình của các công ty chứng khoán
        /// </summary>
        public float TargetPrice { get; set; } = -1;

        /// <summary>
        /// Ghi chú giá dự phong trung bình của các công ty chứng khoán <see cref="AnalyticsNote"/>
        /// </summary>
        public byte[]? TargetPriceNotes { get; set; } = null;

        /// <summary>
        /// Tổng hợp điểm đánh giá, 
        /// tạm thời là tổng của MacroeconomicsScore * 1, CompanyValueScore * 1, CompanyGrowthScore * 1, StockScore * 1. Tưởng lai có thể thay đổi các biến số để đánh trọng số cao hơn cho các score
        /// </summary>
        public int TotalScore => MarketScore + CompanyValueScore + CompanyGrowthScore + StockScore;
    }
}