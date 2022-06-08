using Pl.Sas.Core.Entities;

namespace Pl.Sas.WebDashboard.Models
{
    public class StockDetailsModel
    {
        /// <summary>
        /// Mã chứng khoán
        /// </summary>
        public string Symbol { get; set; } = null!;

        /// <summary>
        /// Danh sách lịch sử giá
        /// </summary>
        public List<StockPrice>? StockPrices { get; set; }

        /// <summary>
        /// Trường hợp đầu tư hiệu quả nhất trong 90 phiên giao dịch.
        /// </summary>
        public AnalyticsResult? AnalyticsResultInfo { get; set; }

        /// <summary>
        /// Danh sách các phương pháp đầu tư
        /// </summary>
        public Dictionary<int, TradingResultViewModel> TradingResults = new();

        /// <summary>
        /// Danh sách diễn giải đánh giá vĩ mô
        /// </summary>
        public List<AnalyticsNote>? MacroeconomicsNote { get; set; } = new();

        /// <summary>
        /// Danh sách diễn giải đánh giá trị doanh nghiệp
        /// </summary>
        public List<AnalyticsNote>? CompanyValueNote { get; set; } = new();

        /// <summary>
        /// Danh sách diễn giải đánh giá tăng trưởng doanh nghiệp
        /// </summary>
        public List<AnalyticsNote>? CompanyGrowthNote { get; set; } = new();

        /// <summary>
        /// Danh sách diễn giải tăng trưởng thị giá
        /// </summary>
        public List<AnalyticsNote>? StockNote { get; set; } = new();

        /// <summary>
        /// Danh sách diễn giải đánh giá của fiin
        /// </summary>
        public List<AnalyticsNote>? FiinNote { get; set; } = new();

        /// <summary>
        /// Danh sách diễn giải đánh giá của vnd
        /// </summary>
        public List<AnalyticsNote>? VndNote { get; set; } = new();

        /// <summary>
        /// Danh sách diễn giải phân tích của các công ty chứng khoán về thị giá
        /// </summary>
        public List<AnalyticsNote>? TargetPriceNotes { get; set; } = new();

        /// <summary>
        /// Chi tiết stock
        /// </summary>
        public Stock? Details { get; set; }

        /// <summary>
        /// Thông tin công ty
        /// </summary>
        public Company? CompanyInfo { get; set; }

        /// <summary>
        /// Thông doanh nghiệp
        /// </summary>
        public string CompanyProfile { get; set; } = "";
    }

    public class TradingResultViewModel
    {
        /// <summary>
        /// Tổng thuế
        /// </summary>
        public float TotalTax { get; set; } = 0;

        /// <summary>
        /// Vốn
        /// </summary>
        public float Capital { get; set; } = 0;

        /// <summary>
        /// Kết quả sau khi chạy quá trình test
        /// </summary>
        public float Profit { get; set; } = 0;

        /// <summary>
        /// % lợi nhuận
        /// </summary>
        public float ProfitPercent { get; set; } = 0;

        /// <summary>
        /// Ghi chú diễn giải đầu tư
        /// </summary>
        public List<KeyValuePair<int, string>>? TradingExplainNotes { get; set; }

        /// <summary>
        /// Số lần thắng
        /// </summary>
        public int WinNumber { get; set; }

        /// <summary>
        /// Số lần thua
        /// </summary>
        public int LoseNumber { get; set; }

        /// <summary>
        /// vị thế tài sản
        /// </summary>
        public string AssetPosition { get; set; } = "100% tiền mặt";
    }
}