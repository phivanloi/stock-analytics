using System;
using System.Collections.Generic;

namespace Pl.Sps.Core.Entities
{
    public class StockTransaction : BaseEntity
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
        /// Chi tiết toàn bộ thông tin khớp lệnh của phiên giao dịch
        /// </summary>
        public byte[] ZipDetails { get; set; } = null;
    }

    public record TradingStockTransaction
    {
        /// <summary>
        /// Chuỗi đại diện cho ngày giao dịch
        /// </summary>
        public string DatePath { get; set; }

        /// <summary>
        /// Danh sách khớp lệnh
        /// </summary>
        public List<StockTransactionDetails> Details { get; set; }
    }

    /// <summary>
    /// Chi tiết về một phiên khớp lệnh
    /// </summary>
    public class StockTransactionDetails
    {
        /// <summary>
        /// Thời gian
        /// </summary>
        public string Time { get; set; }

        /// <summary>
        /// Giá
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Giá tham chiếu
        /// </summary>
        public decimal RefPrice { get; set; }

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