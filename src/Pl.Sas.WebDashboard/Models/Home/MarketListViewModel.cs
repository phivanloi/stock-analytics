using Pl.Sas.Core.Entities;

namespace Pl.Sas.WebDashboard.Models
{
    public class MarketListViewModel
    {
        /// <summary>
        /// Danh sách các mã chứng khoán người dùng theo dõi
        /// </summary>
        public List<string> UserFollowSymbols { get; set; } = null!;

        /// <summary>
        /// Danh sách dữ liệu phân tích
        /// </summary>
        public List<StockView> StockViews { get; set; } = null!;

        /// <summary>
        /// Lãi suất ngân hàng 12 tháng cao nhất
        /// </summary>
        public float BankInterestRate12 { get; set; }
    }
}