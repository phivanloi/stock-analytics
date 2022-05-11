using Pl.Sas.Core.Entities;
using System.Collections.Generic;

namespace Pl.Sas.WebDashboard.Models
{
    public class MarketListViewModel
    {
        public List<string> UserFollowSymbols { get; set; } = null!;

        public List<StockView> StockViews { get; set; } = null!;

        /// <summary>
        /// Phương pháp hiển thị
        /// </summary>
        public int Principle { get; set; } = 1;

        /// <summary>
        /// Lãi suất ngân hàng 12 tháng cao nhất
        /// </summary>
        public float BankInterestRate12 { get; set; }
    }
}