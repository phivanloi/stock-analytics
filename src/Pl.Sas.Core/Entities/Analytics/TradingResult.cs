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
        /// <para>0 => Thử nghiệm</para>
        /// <para>1 => Phương pháp chính</para>
        /// <para>2 => Tích sản khi giá rẻ</para>
        /// <para>3 => Mua và nắm giữ</para>
        /// </summary>
        public int Principle { get; set; } = 1;

        /// <summary>
        /// Vốn cố định ban đầu, 100tr vnd
        /// </summary>
        public float FixedCapital { get; set; } = 100000000;

        /// <summary>
        /// Số lần thắng
        /// </summary>
        public int WinNumber { get; set; }

        /// <summary>
        /// Số lần thua
        /// </summary>
        public int LoseNumber { get; set; }

        /// <summary>
        /// Kết quả sau khi chạy quá trình test
        /// </summary>
        public float Profit { get; set; } = 0;

        /// <summary>
        /// Phần trăm lợi nhuận
        /// </summary>
        public float ProfitPercent => (Profit - FixedCapital) * 100 / FixedCapital;

        /// <summary>
        /// Tổng thuế phí giao dịch
        /// </summary>
        public float TotalTax { get; set; } = 0;

        /// <summary>
        /// Trạng thái mua hôm nay
        /// </summary>
        public bool IsBuy { get; set; }

        /// <summary>
        /// Giá mua nếu là trạng thái mua
        /// </summary>
        public float BuyPrice { get; set; }

        /// <summary>
        /// Trạng thái bán hôm nay
        /// </summary>
        public bool IsSell { get; set; }

        /// <summary>
        /// Giá bán nếu là trạng thái bán
        /// </summary>
        public float SellPrice { get; set; }

        /// <summary>
        /// vị thế tài sản
        /// </summary>
        public string AssetPosition { get; set; } = "100% tiền mặt";

        /// <summary>
        /// Ghi chú diễn giải đầu tư <see cref="string"/>
        /// </summary>
        public byte[]? TradingNotes { get; set; } = null;
    }
}