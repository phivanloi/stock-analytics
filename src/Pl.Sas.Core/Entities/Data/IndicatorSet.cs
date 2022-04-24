namespace Pl.Sas.Core.Entities
{
    /// <summary>
    /// Tập hợp các chỉ số dùng cho trading
    /// </summary>
    public class IndicatorSet
    {
        /// <summary>
        /// mã chứng khoán
        /// </summary>
        public string Symbol { get; set; } = null!;

        /// <summary>
        /// ngày giao dịch
        /// </summary>
        public DateTime TradingDate { get; set; } = Utilities.GetTradingDate();

        /// <summary>
        /// Chuỗi đại diện cho ngày giao dịch
        /// </summary>
        public string DatePath { get; set; } = null!;

        /// <summary>
        /// Tập hợp các chỉ báo tên chỉ báo và giá trị
        /// </summary>
        public Dictionary<string, decimal> Values { get; set; } = new Dictionary<string, decimal>();
    }
}