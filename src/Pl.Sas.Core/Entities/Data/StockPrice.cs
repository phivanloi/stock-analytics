using System;

namespace Pl.Sps.Core.Entities
{
    public class StockPrice : BaseEntity
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
        /// Giá thay đổi
        /// </summary>
        public decimal PriceChange { get; set; }

        /// <summary>
        /// Giá mỗi khi thay đổi
        /// </summary>
        public decimal PerPriceChange { get; set; }

        /// <summary>
        /// Giá trần
        /// </summary>
        public decimal CeilingPrice { get; set; }

        /// <summary>
        /// Giá sàn
        /// </summary>
        public decimal FloorPrice { get; set; }

        /// <summary>
        /// Giá tham chiếu
        /// </summary>
        public decimal RefPrice { get; set; }

        /// <summary>
        /// Giá mở cửa
        /// </summary>
        public decimal OpenPrice { get; set; }

        /// <summary>
        /// Giá cao nhất
        /// </summary>
        public decimal HighestPrice { get; set; }

        /// <summary>
        /// Giá thấp nhất
        /// </summary>
        public decimal LowestPrice { get; set; }

        /// <summary>
        /// Giá đóng cửa
        /// </summary>
        public decimal ClosePrice { get; set; }

        /// <summary>
        /// Giá trung bình
        /// </summary>
        public decimal AveragePrice { get; set; }

        /// <summary>
        /// Giá đóng cửa điểu chỉnh
        /// </summary>
        public decimal ClosePriceAdjusted { get; set; }

        /// <summary>
        /// Khối lượng khớp lệnh
        /// </summary>
        public decimal TotalMatchVol { get; set; }

        /// <summary>
        /// Tổng giá trị khớp lệnh
        /// </summary>
        public decimal TotalMatchVal { get; set; }

        /// <summary>
        /// Tổng giá trị thỏa thuận
        /// </summary>
        public decimal TotalDealVal { get; set; }

        /// <summary>
        /// Khối lượng thỏa thuẩn
        /// </summary>
        public decimal TotalDealVol { get; set; }

        /// <summary>
        /// Tổng khối lượng mua nước ngoài
        /// </summary>
        public decimal ForeignBuyVolTotal { get; set; }

        /// <summary>
        /// Tổng giá trị phòng của nước ngoài
        /// </summary>
        public decimal ForeignCurrentRoom { get; set; }

        /// <summary>
        /// Tổng khối lượng bán nước ngoài
        /// </summary>
        public decimal ForeignSellVolTotal { get; set; }

        /// <summary>
        /// Tổng giá trị mua nước ngoài
        /// </summary>
        public decimal ForeignBuyValTotal { get; set; }

        /// <summary>
        /// Tổng giá trị bán nước ngoài
        /// </summary>
        public decimal ForeignSellValTotal { get; set; }

        /// <summary>
        /// Tổng số người mua
        /// </summary>
        public decimal TotalBuyTrade { get; set; }

        /// <summary>
        /// Tổng số giá trị người mua
        /// </summary>
        public decimal TotalBuyTradeVol { get; set; }

        /// <summary>
        /// Tổng số người bán
        /// </summary>
        public decimal TotalSellTrade { get; set; }

        /// <summary>
        /// Tổng giá trị bán
        /// </summary>
        public decimal TotalSellTradeVol { get; set; }

        /// <summary>
        /// Tổng mua, bán số lượng tối đa
        /// </summary>
        public decimal NetBuySellVol { get; set; }

        /// <summary>
        /// Tổng giá trị mua, bán tối đa
        /// </summary>
        public decimal NetBuySellVal { get; set; }
    }

    public class StockPriceAdj
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
        /// Giá mở cửa
        /// </summary>
        public decimal OpenPrice { get; set; }

        /// <summary>
        /// Giá cao nhất
        /// </summary>
        public decimal HighestPrice { get; set; }

        /// <summary>
        /// Giá thấp nhất
        /// </summary>
        public decimal LowestPrice { get; set; }

        /// <summary>
        /// Giá đóng cửa
        /// </summary>
        public decimal ClosePrice { get; set; }

        /// <summary>
        /// Khối lượng khớp lệnh
        /// </summary>
        public decimal TotalMatchVol { get; set; }
    }
}