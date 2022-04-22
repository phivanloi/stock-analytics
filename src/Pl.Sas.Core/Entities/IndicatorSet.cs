using System;
using System.Collections.Generic;

namespace Pl.Sas.Core.Entities
{
    public class IndicatorSet
    {
        /// <summary>
        /// mã chứng khoán
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// ngày giao dịch
        /// </summary>
        public DateTime TradingDate { get; set; }

        /// <summary>
        /// Chuỗi đại diện cho ngày giao dịch
        /// </summary>
        public string DatePath { get; set; }

        /// <summary>
        /// Tập hợp các chỉ báo tên chỉ báo và giá trị
        /// </summary>
        public Dictionary<string, decimal> Values { get; set; } = new Dictionary<string, decimal>();
    }
}