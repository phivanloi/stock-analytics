using Pl.Sas.Core.Entities;
using System;
using System.Collections.Generic;

namespace Pl.Sas.WebDashboard.Models
{
    public class StockDetailsModel
    {
        /// <summary>
        /// Mã chứng khoán
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Danh sách lịch sử giá
        /// </summary>
        public List<StockPrice> StockPrices { get; set; }

        /// <summary>
        /// Lợi nhuận theo giai đoạn( chưa được thực hiện)
        /// </summary>
        public List<ProfitStage> ProfitStages { get; set; } = new();

        /// <summary>
        /// Trường hợp đầu tư hiệu quả nhất trong 90 phiên giao dịch.
        /// </summary>
        public AnalyticsResult AnalyticsResultInfo { get; set; }

        /// <summary>
        /// Danh sách các phương pháp đầu tư
        /// </summary>
        public Dictionary<int, TradingResultViewModel> TradingResults = new();

        /// <summary>
        /// Danh sách diễn giải đánh giá vĩ mô
        /// </summary>
        public List<AnalyticsMessage> MacroeconomicsNote { get; set; } = new();

        /// <summary>
        /// Danh sách diễn giải đánh giá trị doanh nghiệp
        /// </summary>
        public List<AnalyticsMessage> CompanyValueNote { get; set; } = new();

        /// <summary>
        /// Danh sách diễn giải đánh giá tăng trưởng doanh nghiệp
        /// </summary>
        public List<AnalyticsMessage> CompanyGrowthNote { get; set; } = new();

        /// <summary>
        /// Danh sách diễn giải tăng trưởng thị giá
        /// </summary>
        public List<AnalyticsMessage> StockNote { get; set; } = new();

        /// <summary>
        /// Danh sách diễn giải đánh giá của fiin
        /// </summary>
        public List<AnalyticsMessage> FiinNote { get; set; } = new();

        /// <summary>
        /// Danh sách diễn giải đánh giá của vnd
        /// </summary>
        public List<AnalyticsMessage> VndNote { get; set; } = new();

        /// <summary>
        /// Danh sách diễn giải phân tích của các công ty chứng khoán về thị giá
        /// </summary>
        public List<AnalyticsMessage> TargetPriceNotes { get; set; } = new();

        /// <summary>
        /// Ghi chú của người dùng
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Chi tiết stock
        /// </summary>
        public Stock Details { get; set; }

        /// <summary>
        /// Thông tin công ty
        /// </summary>
        public Company CompanyInfo { get; set; }

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
        public decimal TotalTax { get; set; } = 0;

        /// <summary>
        /// Vốn
        /// </summary>
        public decimal Capital { get; set; } = 0;

        /// <summary>
        /// Kết quả sau khi chạy quá trình test
        /// </summary>
        public decimal Profit { get; set; } = 0;

        /// <summary>
        /// % lợi nhuận
        /// </summary>
        public decimal ProfitPercent { get; set; } = 0;

        /// <summary>
        /// Ghi chú diễn giải đầu tư
        /// </summary>
        public List<KeyValuePair<int, string>> TradingExplainNotes { get; set; }
    }

    /// <summary>
    /// Lợi nhuận theo giai đoạn
    /// </summary>
    public class ProfitStage
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        /// <summary>
        /// Số phiên giao dịch
        /// </summary>
        public int NumberOfTradingDay { get; set; }

        public decimal StartPrice { get; set; }

        public decimal EndPrice { get; set; }

        /// <summary>
        /// % Lợi nhuận
        /// </summary>
        public decimal ProfitPercent { get; set; }
    }
}