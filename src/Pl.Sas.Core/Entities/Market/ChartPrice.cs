namespace Pl.Sas.Core.Entities
{
    /// <summary>
    /// Bảng chart
    /// </summary>
    public class ChartPrice : BaseEntity
    {
        /// <summary>
        /// Mã chứng khoán <see cref="Stock"/>
        /// </summary>
        public string Symbol { get; set; } = null!;

        /// <summary>
        /// ngày giao dịch
        /// </summary>
        public DateTime TradingDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Loại chart
        /// D => ngày
        /// </summary>
        public string Type { get; set; } = "D";

        /// <summary>
        /// Giá mở cửa
        /// </summary>
        public float OpenPrice { get; set; }

        /// <summary>
        /// Giá cao nhất
        /// </summary>
        public float HighestPrice { get; set; }

        /// <summary>
        /// Giá thấp nhất
        /// </summary>
        public float LowestPrice { get; set; }

        /// <summary>
        /// Giá đóng cửa
        /// </summary>
        public float ClosePrice { get; set; }

        /// <summary>
        /// Khối lượng khớp lệnh
        /// </summary>
        public float TotalMatchVol { get; set; }
    }
}