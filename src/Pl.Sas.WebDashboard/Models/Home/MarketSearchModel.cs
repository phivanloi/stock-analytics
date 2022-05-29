namespace Pl.Sas.WebDashboard.Models
{
    public class MarketSearchModel
    {
        /// <summary>
        /// thứ tự sắp xếp
        /// </summary>
        public string? Ordinal { get; set; }

        /// <summary>
        /// Sàn giao dịch
        /// </summary>
        public string? Exchange { get; set; }

        /// <summary>
        /// Lĩnh vực
        /// </summary>
        public string? IndustryCode { get; set; }

        /// <summary>
        /// Mã chứng khoán
        /// </summary>
        public string? Symbol { get; set; }

        /// <summary>
        /// Vùng
        /// </summary>
        public string? Zone { get; set; }
    }
}