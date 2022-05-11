namespace Pl.Sas.Core.Entities
{
    /// <summary>
    /// bảng lưu kết quả giao dịch thử nghiệm
    /// </summary>
    public class TradingResult : BaseEntity
    {
        /// <summary>
        /// Mã chứng khoán
        /// </summary>
        public string Symbol { get; set; } = null!;

        /// <summary>
        /// Phương pháp
        /// <para>1 => Dài hạn</para>
        /// <para>2 => Trung hạn</para>
        /// <para>3 => Ngắn hạn</para>
        /// <para>4 => Thử nghiệm</para>
        /// </summary>
        public int Principle { get; set; } = 1;

        /// <summary>
        /// Đánh giá mua cho phiên này
        /// </summary>
        public bool IsBuy { get; set; } = false;

        /// <summary>
        /// Giá mua vào cho phiên này
        /// </summary>
        public float BuyPrice { get; set; } = 0;

        /// <summary>
        /// Đánh giá bán cho phiên này
        /// </summary>
        public bool IsSell { get; set; } = false;

        /// <summary>
        /// Giá bán
        /// </summary>
        public float SellPrice { get; set; } = 0;

        /// <summary>
        /// Vốn cố định ban đầu, 100tr vnd
        /// </summary>
        public float Capital { get; set; } = 100000000;

        /// <summary>
        /// Kết quả sau khi chạy quá trình test
        /// </summary>
        public float Profit { get; set; } = 0;

        /// <summary>
        /// Phần trăm lợi nhuận
        /// </summary>
        public float ProfitPercent => (Profit - Capital) * 100 / Capital;

        /// <summary>
        /// Tổng thuế phí giao dịch
        /// </summary>
        public float TotalTax { get; set; } = 0;

        /// <summary>
        /// Ghi chú diễn giải đầu tư <see cref="string"/>
        /// </summary>
        public byte[]? TradingNotes { get; set; } = null;
    }
}