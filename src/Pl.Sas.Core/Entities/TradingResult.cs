using System;

namespace Pl.Sas.Core.Entities
{
    public class TradingResult : BaseEntity
    {
        /// <summary>
        /// Mã chứng khoán
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Phương pháp
        /// <para>1 => Dài hạn</para>
        /// <para>2 => Trung hạn</para>
        /// <para>3 => Ngắn hạn</para>
        /// <para>4 => Thử nghiệm</para>
        /// </summary>
        public int Principle { get; set; } = 1;

        /// <summary>
        /// Ngày phân tích
        /// </summary>
        public DateTime TradingDate { get; set; } = Utilities.GetTradingDate();

        /// <summary>
        /// Chuỗi đại diện cho ngày phân tích
        /// </summary>
        public string DatePath { get; set; } = Utilities.GetTradingDatePath();

        /// <summary>
        /// Đánh giá mua cho phiên này
        /// </summary>
        public bool IsBuy { get; set; } = false;

        /// <summary>
        /// Giá mua vào cho phiên này
        /// </summary>
        public decimal BuyPrice { get; set; } = 0;

        /// <summary>
        /// Đánh giá bán cho phiên này
        /// </summary>
        public bool IsSell { get; set; } = false;

        /// <summary>
        /// Giá bán
        /// </summary>
        public decimal SellPrice { get; set; } = 0;

        /// <summary>
        /// Kết quả sau khi chạy quá trình test
        /// </summary>
        public decimal Profit { get; set; } = 0;

        /// <summary>
        /// % lợi nhuận
        /// </summary>
        public decimal ProfitPercent { get; set; } = 0;

        /// <summary>
        /// Vốn cố định ban đầu
        /// </summary>
        public decimal Capital { get; set; } = 0;

        /// <summary>
        /// Tổng thuế phí giao dịch
        /// </summary>
        public decimal TotalTax { get; set; } = 0;

        /// <summary>
        /// Ghi chú diễn giải đầu tư
        /// </summary>
        public byte[] ZipExplainNotes { get; set; } = null;
    }
}