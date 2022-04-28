namespace Pl.Sas.Core.Entities
{
    /// <summary>
    /// bảng theo dõi quá trình xử lý cổ phiếu
    /// </summary>
    public class StockTracking : BaseEntity
    {
        public StockTracking() { }

        public StockTracking(string symbol)
        {
            Symbol = symbol;
        }

        /// <summary>
        /// Mã chứng khoán <see cref="Stock"/>
        /// </summary>
        public string Symbol { get; set; } = null!;

        /// <summary>
        /// Trạng thái tải dữ liệu
        /// <para>ok => thành công, khác là có vấn đề</para>
        /// </summary>
        public string DownloadStatus { get; set; } = "";

        /// <summary>
        /// Ngày download dữ liệu
        /// </summary>
        public DateTime DownloadDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Trạng thái dữ liệu
        /// <para>ok => đầy đủ, khác là có vấn đề</para>
        /// </summary>
        public string DataStatus { get; set; } = "";

        /// <summary>
        /// Ngày kiểm tra dữ liệu
        /// </summary>
        public DateTime DataDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Trạng thái phân tích
        /// <para>ok => phân tích thành công, khác là có vấn đề</para>
        /// </summary>
        public string AnalyticsStatus { get; set; } = "";

        /// <summary>
        /// Ngày phân tích
        /// </summary>
        public DateTime AnalyticsDate { get; set; } = DateTime.Now;
    }
}