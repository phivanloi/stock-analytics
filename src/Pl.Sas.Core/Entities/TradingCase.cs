using System.Collections.Generic;

namespace Pl.Sas.Core.Entities
{
    public class TradingCase
    {
        /// <summary>
        /// Vốn cố định ban đầu
        /// </summary>
        public decimal FixedCapital { get; set; }

        /// <summary>
        /// Kết quả sau khi chạy quá trình test
        /// </summary>
        public decimal TradingTestResult { get; set; }

        /// <summary>
        /// Ghi chú diễn giải đầu tư với key -1, thua, 0 hòa hoặc không giao dịch, 1 là thắng. Value là nội dung ghi chú
        /// </summary>
        public List<KeyValuePair<int, string>> ExplainNotes { get; set; } = new();

        /// <summary>
        /// Tổng thuế phí giao dịch
        /// </summary>
        public decimal TotalTax { get; set; }

        /// <summary>
        /// Phần trăm lợi nhuận
        /// </summary>
        public decimal ProfitPercent => (TradingTestResult - FixedCapital) * 100 / FixedCapital;

        /// <summary>
        /// Số phiên chỉ báo ema đầu cho lênh mua
        /// </summary>
        public int FirstEmaBuy { get; set; }

        /// <summary>
        /// Số phiên chỉ báo ema sau cho lệnh mua
        /// </summary>
        public int SecondEmaBuy { get; set; }

        /// <summary>
        /// Số phiên chỉ báo ema đầu
        /// </summary>
        public int FirstEmaSell { get; set; }

        /// <summary>
        /// Số phiên chỉ báo ema sau
        /// </summary>
        public int SecondEmaSell { get; set; }

        /// <summary>
        /// Chỉ báo Stochastic được sử dụng trong phương pháp
        /// </summary>
        public int Stochastic { get; set; } = 14;

        /// <summary>
        /// Mức chặn chỉ báo stochastic để bán không được thấp hơn mức này
        /// </summary>
        public int HighestStochasticSell { get; set; } = 60;

        /// <summary>
        /// Mức chặn cho chỉ báo stochastic để mua không được vượt quá mức này
        /// </summary>
        public int LowestStochasticBuy { get; set; } = 30;
    }
}