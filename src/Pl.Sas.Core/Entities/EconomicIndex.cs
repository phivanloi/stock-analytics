using System;

namespace Pl.Sas.Core.Entities
{
    /// <summary>
    /// Các chỉ số kinh tế vĩ mô chung
    /// </summary>
    public class EconomicIndex : BaseEntity
    {
        /// <summary>
        /// Ngày phân tích
        /// </summary>
        public DateTime TradingDate { get; set; } = Utilities.GetTradingDate();

        /// <summary>
        /// Chuỗi đại diện cho ngày thống kê. Dự kiến mỗi ngày giao dịch trong tuần sẽ là 1 lần thống kê.
        /// </summary>
        public string DatePath { get; set; } = Utilities.GetTradingDatePath();

        /// <summary>
        /// Khóa truy cập
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Giá trị của chỉ số không âm
        /// </summary>
        public float Value { get; set; }

        /// <summary>
        /// Mô tả cho chỉ số
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Mặc định là ảnh hưởng toàn bộ thị trường, nếu không thì liệt kê cụ thể mã ngành
        /// </summary>
        public string IndustriesUp { get; set; } = "allmarket";

        /// <summary>
        /// Mặc định là ảnh hưởng toàn bộ thị trường, nếu không thì liệt kê cụ thể mã ngành
        /// </summary>
        public string IndustriesDown { get; set; } = "allmarket";

        /// <summary>
        /// Mặc định là ảnh hưởng toàn bộ thị trường, nếu không thì liệt kê cụ thể mã ngành
        /// </summary>
        public string SymbolsUp { get; set; } = "allmarket";

        /// <summary>
        /// Mặc định là ảnh hưởng toàn bộ thị trường, nếu không thì liệt kê cụ thể mã ngành
        /// </summary>
        public string SymbolsDown { get; set; } = "allmarket";
    }
}