namespace Pl.Sas.Core.Entities
{
    /// <summary>
    /// Bảng lịch sử giá của cổ phiếu
    /// </summary>
    public class StockPrice : BaseEntity
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
        /// trading date path
        /// </summary>
        public string DatePath => TradingDate.ToString("yyyyMMdd");

        /// <summary>
        /// Giá thay đổi
        /// </summary>
        public float PriceChange { get; set; }

        /// <summary>
        /// Giá mỗi khi thay đổi
        /// </summary>
        public float PerPriceChange { get; set; }

        /// <summary>
        /// Giá trần
        /// </summary>
        public float CeilingPrice { get; set; }

        /// <summary>
        /// Giá sàn
        /// </summary>
        public float FloorPrice { get; set; }

        /// <summary>
        /// Giá tham chiếu
        /// </summary>
        public float RefPrice { get; set; }

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
        /// Giá trung bình
        /// </summary>
        public float AveragePrice { get; set; }

        /// <summary>
        /// Giá đóng cửa điểu chỉnh
        /// </summary>
        public float ClosePriceAdjusted { get; set; }

        /// <summary>
        /// Khối lượng khớp lệnh
        /// </summary>
        public float TotalMatchVol { get; set; }

        /// <summary>
        /// Tổng giá trị khớp lệnh
        /// </summary>
        public float TotalMatchVal { get; set; }

        /// <summary>
        /// Tổng giá trị thỏa thuận
        /// </summary>
        public float TotalDealVal { get; set; }

        /// <summary>
        /// Khối lượng thỏa thuẩn
        /// </summary>
        public float TotalDealVol { get; set; }

        /// <summary>
        /// Tổng khối lượng mua nước ngoài
        /// </summary>
        public float ForeignBuyVolTotal { get; set; }

        /// <summary>
        /// Tổng giá trị phòng của nước ngoài
        /// </summary>
        public float ForeignCurrentRoom { get; set; }

        /// <summary>
        /// Tổng khối lượng bán nước ngoài
        /// </summary>
        public float ForeignSellVolTotal { get; set; }

        /// <summary>
        /// Tổng giá trị mua nước ngoài
        /// </summary>
        public float ForeignBuyValTotal { get; set; }

        /// <summary>
        /// Tổng giá trị bán nước ngoài
        /// </summary>
        public float ForeignSellValTotal { get; set; }

        /// <summary>
        /// Tổng số người mua
        /// </summary>
        public float TotalBuyTrade { get; set; }

        /// <summary>
        /// Tổng số giá trị người mua
        /// </summary>
        public float TotalBuyTradeVol { get; set; }

        /// <summary>
        /// Tổng số người bán
        /// </summary>
        public float TotalSellTrade { get; set; }

        /// <summary>
        /// Tổng giá trị bán
        /// </summary>
        public float TotalSellTradeVol { get; set; }

        /// <summary>
        /// Tổng mua, bán số lượng tối đa
        /// </summary>
        public float NetBuySellVol { get; set; }

        /// <summary>
        /// Tổng giá trị mua, bán tối đa
        /// </summary>
        public float NetBuySellVal { get; set; }
    }
}