using System;
using System.Collections.Generic;

namespace Pl.Sas.Core.Entities
{
    public class StockTransaction : BaseEntity
    {
        /// <summary>
        /// mã chứng khoán
        /// </summary>
        public string Symbol { get; set; } = null!;

        /// <summary>
        /// Ngày giao dịch
        /// </summary>
        public DateTime TradingDate { get; set; }

        /// <summary>
        /// Chi tiết toàn bộ thông tin khớp lệnh của phiên giao dịch
        /// </summary>
        public byte[]? ZipDetails { get; set; } = null;
    }

    /// <summary>
    /// Chi tiết về một phiên khớp lệnh
    /// </summary>
    public class StockTransactionDetails
    {
        /// <summary>
        /// Thời gian
        /// </summary>
        public string Time { get; set; } = null!;

        /// <summary>
        /// Giá
        /// </summary>
        public float Price { get; set; }

        /// <summary>
        /// Giá tham chiếu
        /// </summary>
        public float RefPrice { get; set; }

        /// <summary>
        /// Khối lượng khớp
        /// </summary>
        public int Vol { get; set; }

        /// <summary>
        /// Khối lượng tích lũy
        /// </summary>
        public int AccumulatedVol { get; set; }
    }
}