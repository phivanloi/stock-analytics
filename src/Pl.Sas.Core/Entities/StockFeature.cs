namespace Pl.Sas.Core.Entities
{
    /// <summary>
    /// Các đặc trưng của một cổ phiếu
    /// </summary>
    public class StockFeature : BaseEntity
    {
        /// <summary>
        /// mã giao dịch
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Số phiên đường trung bình ema đầu cho điều kiện mua
        /// </summary>
        public int FirstEmaBuy { get; set; } = 1;

        /// <summary>
        /// Số phiên đường trung bình ema thứ hai cho điều kiện mua
        /// </summary>
        public int SecondEmaBuy { get; set; } = 3;

        /// <summary>
        /// Số phiên đường trung bình ema đầu cho điều kiện bán
        /// </summary>
        public int FirstEmaSell { get; set; } = 1;

        /// <summary>
        /// Số phiên đường trung bình ema thứ hai cho điều kiện bán
        /// </summary>
        public int SecondEmaSell { get; set; } = 3;

        /// <summary>
        /// Số phiên Stochastic cho lệnh mua
        /// </summary>
        public int StochasticBuy { get; set; } = 14;

        /// <summary>
        /// Giá trị Stochastic chặn trên khi mua
        /// </summary>
        public int HighestStochasticBuy { get; set; } = 25;

        /// <summary>
        /// Giá trị Stochastic chặn dưới khi mua
        /// </summary>
        public int LowestStochasticBuy { get; set; } = 25;

        /// <summary>
        /// Số phiên stochastic cho lệnh bán
        /// </summary>
        public int StochasticSell { get; set; } = 14;

        /// <summary>
        /// Giá trị Stochastic chặn trên khi bán
        /// </summary>
        public int HighestStochasticSell { get; set; } = 90;

        /// <summary>
        /// Giá trị Stochastic chặn dưới khi bán
        /// </summary>
        public int LowestStochasticSell { get; set; } = 75;

        /// <summary>
        /// những khung giờ mua tốt
        /// </summary>
        public string GoodBuyTimes { get; set; } = "";

        /// <summary>
        /// Khung giờ bán tốt
        /// </summary>
        public string GoodSellTimes { get; set; } = "";

        /// <summary>
        /// Điểm đánh giá chứng khoán tự động
        /// </summary>
        public int AutoRank { get; set; } = 0;

        /// <summary>
        /// Điểm đánh giá chứng khoán bằng con người
        /// </summary>
        public int Rank { get; set; } = 0;

        /// <summary>
        /// Số người thắng trong phân tích trading thử nghiệm
        /// </summary>
        public long NumberWinTrander { get; set; } = 0;

        /// <summary>
        /// Số người thua trong phân tíc trading thử nghiệm
        /// </summary>
        public long NumberLoseTrander { get; set; } = 0;
    }
}