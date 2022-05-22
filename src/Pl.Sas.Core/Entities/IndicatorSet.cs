namespace Pl.Sas.Core.Entities
{
    /// <summary>
    /// Tập hợp các chỉ số dùng cho trading
    /// </summary>
    public class IndicatorSet
    {
        /// <summary>
        /// ngày giao dịch
        /// </summary>
        public DateTime TradingDate { get; set; } = Utilities.GetTradingDate();

        /// <summary>
        /// Giá đóng cửa
        /// </summary>
        public float ClosePrice { get; set; }

        /// <summary>
        /// Tập hợp các chỉ báo tên chỉ báo và giá trị
        /// </summary>
        public Dictionary<string, float> Values { get; set; } = new Dictionary<string, float>();

        public float S(string f, string l) => Values[f] - Values[l];
    }
}