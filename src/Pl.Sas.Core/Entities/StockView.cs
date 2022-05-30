using System.Text.Json;
using Pl.Sas.Core;

namespace Pl.Sas.Core.Entities
{
    public class StockView
    {
        #region Description
        /// <summary>
        /// mã giao dịch
        /// </summary>
        public string Symbol { get; set; } = null!;

        /// <summary>
        /// Mã ngành hoạt động chính
        /// </summary>
        public string IndustryCode { get; set; } = null!;

        /// <summary>
        /// Mô tả, tên công ty
        /// </summary>m
        public string? Description { get; set; }

        /// <summary>
        /// Sàn niêm yết
        /// </summary>
        public string Exchange { get; set; } = null!;
        #endregion

        #region Macroeconomic
        /// <summary>
        /// Điểm đánh giá trạng thái kinh tế vĩ mô với thị trường chứng khoán, dưới đây liệt kê các vấn đề tác động.
        /// <para>- Dịch bệnh</para>
        /// <para>- Thiên tai</para>
        /// <para>- Chiến tranh</para>
        /// <para>- Sự hỗ trợ của thể chế</para>
        /// <para>- Tâm lý nhà đầu tư</para>
        /// <para>- Các chỉ số vĩ mô</para>
        /// <para>- Đánh giá su thế ngành</para>
        /// </summary>
        public int MacroeconomicsScore { get; set; }

        /// <summary>
        /// Điểm mã ngành tổng hợp của mã ngành đánh giá và mã ngành phân tích tự động.
        /// </summary>
        public int IndustryRank { get; set; }
        #endregion

        #region Company Value
        /// <summary>
        /// Điểm đánh giá giá trị cua công ty
        /// </summary>
        public int CompanyValueScore { get; set; }

        /// <summary>
        /// <para>Chỉ số EPS</para>
        /// <para>lợi nhuận (thu nhập) trên mỗi cổ phiếu.</para>
        /// <para>EPS = (Thu nhập ròng - cổ tức cổ phiếu ưu đãi) / lượng cổ phiếu bình quân đang lưu thông.</para>
        /// </summary>
        public float Eps { get; set; }

        /// <summary>
        /// <para>Chỉ số pe</para>
        /// <para>Dùng để đo lường mối quan hệ giữa Giá thị trường của cổ phiếu (Price) và Thu nhập trên một cổ phiếu (EPS).</para>
        /// <para>P/E = Giá thị trường của cổ phiếu (Price)/Thu nhập trên một cổ phiếu (EPS)</para>
        /// <para>Chỉ số P/E thể hiện mức giá mà nhà đầu tư sẵn sàng bỏ ra cho một đồng lợi nhuận thu được từ cổ phiếu.</para>
        /// </summary>
        public float Pe { get; set; }

        /// <summary>
        /// Chỉ số pb
        /// <para>P/B = Giá cổ phiếu/Tổng giá trị tài sản – giá trị tài sản vô hình – nợ</para>
        /// <para>Chỉ số P/B (Price-to-Book ratio – Giá/Giá trị sổ sách) là tỷ lệ được sử dụng để so sánh giá của một cổ phiếu so với giá trị ghi sổ của cổ phiếu đó.</para>
        /// <para>https://vnexpress.net/chi-so-p-b-co-y-nghia-nhu-the-nao-2695409.html</para>
        /// </summary>
        public float Pb { get; set; }

        /// <summary>
        /// <para>Chỉ số roe</para>
        /// <para>Return On Equity (ROE) hay lợi nhuận trên vốn chủ sở hữu là chỉ số đo lường mức độ hiệu quả của việc sử dụng vốn chủ sở hữu trong doanh nghiệp.</para>
        /// <para>roe = Lợi nhuận sau thuế(Earning) / Vốn chủ sở hữu bình quân(Equity)</para>
        /// </summary>
        public float Roe { get; set; }

        /// <summary>
        /// <para>Chỉ số roa</para>
        /// <para>Return On Asset (ROA) hay lợi nhuận trên tổng tài sản là chỉ số đo lường mức độ hiệu quả của việc sử dụng tài sản của doanh nghiệp.</para>
        /// <para>roe = Lợi nhuận sau thuế(Earning) / Tổng tài sản bình quân</para>
        /// </summary>
        public float Roa { get; set; }

        /// <summary>
        /// <para>Vốn hóa thị trường</para>
        /// <para>là tổng giá trị của số cổ phần của một công ty niêm yết</para>
        /// <para>Vốn hóa = Giá 1 cổ phiếu X số lượng cổ phiếu đang lưu hành.</para>
        /// </summary>
        public float MarketCap { get; set; }
        #endregion

        #region Company Growth
        /// <summary>
        /// Điểm đánh giá tăng trưởng doanh nghiệp
        /// </summary>
        public int CompanyGrowthScore { get; set; }

        /// <summary>
        /// % tăng trưởng doanh thu 3 năm gần đây nhất
        /// </summary>
        public float YearlyRevenueGrowthPercent { get; set; }

        /// <summary>
        /// % tăng trưởng lợi nhuận 3 năm gần đây nhất
        /// </summary>
        public float YearlyProfitGrowthPercent { get; set; }
        #endregion

        #region Price Crowth

        /// <summary>
        /// Điểm đánh giá giao dịch
        /// </summary>
        public int StockScore { get; set; }

        /// <summary>
        /// <para>Chỉ số BETA đo lường khả năng biến động giá so với thị trường(vnindex) </para>
        /// <para>+ Bằng 1, mức biến động của giá chứng khoán này sẽ bằng với mức biến động của thị trường.</para>
        /// <para>+ Nhỏ hơn 1, mức độ biến động của giá chứng khoán này thấp hơn mức biến động của thị trường.</para>
        /// <para>+ Lớn hơn 1: mức độ biến động giá của chứng khoán này lớn hơn mức biến động của thị trường.</para>
        /// <para>Cụ thể hơn, nếu một chứng khoán có beta bằng 1,2 thì trên lý thuyết mức biến động của chứng khoán này sẽ cao hơn mức biến động chung của thị trường 20%.</para>
        /// <para>http://s.cafef.vn/help/hesobeta.aspx</para>
        /// </summary>
        public float Beta { get; set; }

        /// <summary>
        /// Biến động giá trong 1 phiên
        /// </summary>
        public float PricePercentConvulsion1 { get; set; }

        /// <summary>
        /// Biến động giá trong 5 phiên
        /// </summary>
        public float PricePercentConvulsion5 { get; set; }

        /// <summary>
        /// Biến động giá trong 10 phiên
        /// </summary>
        public float PricePercentConvulsion10 { get; set; }

        /// <summary>
        /// Biến động giá trong 30 phiên
        /// </summary>
        public float PricePercentConvulsion30 { get; set; }

        /// <summary>
        /// Biến động giá trong 50 phiên
        /// </summary>
        public float PricePercentConvulsion50 { get; set; }

        /// <summary>
        /// Biến động giá trong 60 phiên
        /// </summary>
        public float PricePercentConvulsion60 { get; set; }

        /// <summary>
        /// Giá đóng cửa gần nhất
        /// </summary>
        public float LastClosePrice { get; set; }

        /// <summary>
        /// Giá đóng cửa cách ngày gần nhất 1 ngày
        /// </summary>
        public float LastOneClosePrice { get; set; }

        /// <summary>
        /// % giá đóng của mới nhất so với giá phiên trước đó
        /// </summary>
        public float LastClosePricePercent => LastClosePrice.GetPercent(LastOneClosePrice);

        /// <summary>
        /// Giá mở cửa phiên cuối cùng
        /// </summary>
        public float LastOpenPrice { get; set; }

        /// <summary>
        /// Giá cao nhất phiên cuối cùng
        /// </summary>
        public float LastHighestPrice { get; set; }

        /// <summary>
        /// Giá thấp nhất phiên cuối cùng
        /// </summary>
        public float LastLowestPrice { get; set; }

        /// <summary>
        /// Giá biến động thấp nhất trong 5 phiên
        /// </summary>
        public float LastHistoryMinLowestPrice { get; set; }

        /// <summary>
        /// Giá biến động cao nhất trong 5 phiên
        /// </summary>
        public float LastHistoryMinHighestPrice { get; set; }

        /// <summary>
        /// Số lần tăng giá liên tuc trong số lịch sử mà trading thử nhiệm tìm được
        /// </summary>
        public int NumberOfClosePriceIncreases { get; set; }

        /// <summary>
        /// Số lần giảm giá liên tuc trong số lịch sử mà trading thử nhiệm tìm được
        /// </summary>
        public int NumberOfClosePriceDecrease { get; set; }

        /// <summary>
        /// Lấy xu thế giá đóng cửa trong số lịch sử mà trading thử nhiệm tìm được
        /// </summary>
        public string GetClosePriceTrend
        {
            get
            {
                if (NumberOfClosePriceIncreases > 0)
                {
                    return $"Tăng {NumberOfClosePriceIncreases} phiên";
                }
                if (NumberOfClosePriceDecrease > 0)
                {
                    return $"Giảm {NumberOfClosePriceDecrease} phiên";
                }
                return "Giữ giá";
            }
        }

        #endregion

        #region Recommendation

        /// <summary>
        /// Điểm đánh giá của fiin
        /// </summary>
        public int FiinScore { get; set; }

        /// <summary>
        /// Điểm đánh giá của vnd
        /// </summary>
        public int VndScore { get; set; }

        /// <summary>
        /// Giá dự phóng trung bình của các công ty chứng khoán
        /// </summary>
        public float TargetPrice { get; set; }

        #endregion

        #region TotalMatchVol
        /// <summary>
        /// Khối lượng khớp lệnh phiên cuối
        /// </summary>
        public float LastTotalMatchVol { get; set; }

        /// <summary>
        /// Khối lượng khớp lệnh phiên cuối cách ngày gần nhất 1 ngày
        /// </summary>
        public float LastOneTotalMatchVol { get; set; }

        /// <summary>
        /// Khối lượng khớp lệnh bình quân 3 phiên cuối
        /// </summary>
        public float LastAvgThreeTotalMatchVol { get; set; }

        /// <summary>
        ///  % Khối lượng khớp lệnh bình quân 3 phiên cuối so với phiên gần nhất
        /// </summary>
        public float ThreeTotalMatchVolPercent => LastTotalMatchVol.GetPercent(LastAvgThreeTotalMatchVol);

        /// <summary>
        /// Khối lượng khớp lệnh bình quân 5 phiên cuối
        /// </summary>
        public float LastAvgFiveTotalMatchVol { get; set; }

        /// <summary>
        ///  % Khối lượng khớp lệnh bình quân 5 phiên cuối so với phiên gần nhất
        /// </summary>
        public float FiveTotalMatchVolPercent => LastTotalMatchVol.GetPercent(LastAvgFiveTotalMatchVol);

        /// <summary>
        /// Khối lượng khớp lệnh bình quân 10 phiên cuối
        /// </summary>
        public float LastAvgTenTotalMatchVol { get; set; }

        /// <summary>
        ///  % Khối lượng khớp lệnh bình quân 10 phiên cuối so với phiên gần nhất
        /// </summary>
        public float TenTotalMatchVolPercent => LastTotalMatchVol.GetPercent(LastAvgTenTotalMatchVol);
        #endregion

        #region Foreign

        /// <summary>
        /// Tổng khối lượng mua nước ngoài
        /// </summary>
        public float LastForeignBuyVolTotal { get; set; }

        /// <summary>
        /// Tổng khối lượng mua nước ngoài bình quân 3 phiên cuối
        /// </summary>
        public float LastAvgThreeForeignBuyVolTotal { get; set; }

        /// <summary>
        /// Tổng khối lượng mua nước ngoài bình quân 5 phiên cuối
        /// </summary>
        public float LastAvgFiveForeignBuyVolTotal { get; set; }

        /// <summary>
        /// Tổng khối lượng mua nước ngoài bình quân 10 phiên cuối
        /// </summary>
        public float LastAvgTenForeignBuyVolTotal { get; set; }

        /// <summary>
        /// Tổng khối lượng bán nước ngoài
        /// </summary>
        public float LastForeignSellVolTotal { get; set; }

        /// <summary>
        /// Tổng khối lượng bán nước ngoài bình quân 3 phiên cuối
        /// </summary>
        public float LastAvgThreeForeignSellVolTotal { get; set; }

        /// <summary>
        /// Tổng khối lượng bán nước ngoài bình quân 5 phiên cuối
        /// </summary>
        public float LastAvgFiveForeignSellVolTotal { get; set; }

        /// <summary>
        /// Tổng khối lượng bán nước ngoài bình quân 10 phiên cuối
        /// </summary>
        public float LastAvgTenForeignSellVolTotal { get; set; }

        /// <summary>
        /// Sức mua khối khoại, hay mua dòng khối khoại ngày cuối cùng
        /// </summary>
        public float LastForeignPurchasingPower => LastForeignBuyVolTotal - LastForeignSellVolTotal;

        /// <summary>
        /// Tổng bình quân sức mua khối ngoại trong vòng 3 ngày gần đây nhất
        /// </summary>
        public float LastAvgThreeForeignPurchasingPower => LastAvgThreeForeignBuyVolTotal - LastAvgThreeForeignSellVolTotal;

        /// <summary>
        /// Tổng bình quân sức mua khối ngoại trong vòng 5 ngày gần đây nhất
        /// </summary>
        public float LastAvgFiveForeignPurchasingPower => LastAvgFiveForeignBuyVolTotal - LastAvgFiveForeignSellVolTotal;

        /// <summary>
        /// Tổng bình quân sức mua khối ngoại trong vòng 10 ngày gần đây nhất
        /// </summary>
        public float LastAvgTenForeignPurchasingPower => LastAvgTenForeignBuyVolTotal - LastAvgTenForeignSellVolTotal;

        /// <summary>
        ///  % khối lượng mua nước ngoài bình quân 3 phiên so với phiên cuối
        /// </summary>
        public float ThreeForeignPurchasingPowerPercent => LastForeignPurchasingPower.GetPercent(LastAvgThreeForeignPurchasingPower);

        /// <summary>
        ///  % khối lượng mua nước ngoài bình quân 5 phiên so với phiên cuối
        /// </summary>
        public float FiveForeignPurchasingPowerPercent => LastForeignPurchasingPower.GetPercent(LastAvgFiveForeignPurchasingPower);

        /// <summary>
        ///  % khối lượng mua nước ngoài bình quân 10 phiên so với phiên cuối
        /// </summary>
        public float TenForeignPurchasingPowerPercent => LastForeignPurchasingPower.GetPercent(LastAvgTenForeignPurchasingPower);

        #endregion

        #region Trading
        /// <summary>
        /// % lợi nhuận bằng phương pháp thử nghiệm
        /// </summary>
        public float ExperimentProfitPercent { get; set; }

        /// <summary>
        /// Trạng thái tài sản và giao dịch hiện tại của phương pháp thử nghiệm
        /// </summary>
        public string ExperimentAssetPosition { get; set; } = null!;

        /// <summary>
        /// % lợi nhuận bằng phương pháp chính
        /// </summary>
        public float MainProfitPercent { get; set; }

        /// <summary>
        /// Trạng thái tài sản và giao dịch hiện tại của phương pháp chính
        /// </summary>
        public string MainAssetPosition { get; set; } = null!;

        /// <summary>
        /// % lợi nhuận khi tích sản
        /// </summary>
        public float AccumulationProfitPercent { get; set; }

        /// <summary>
        /// Trạng thái tài sản và giao dịch hiện tại của phương pháp tích sản
        /// </summary>
        public string AccumulationAssetPosition { get; set; } = null!;

        /// <summary>
        /// % lợi nhuận khi chỉ mua và giữ
        /// </summary>
        public float BuyAndHoldProfitPercent { get; set; }

        /// <summary>
        /// Trạng thái tài sản và giao dịch hiện tại của phương pháp mua và nắm giữ
        /// </summary>
        public string BuyAndHoldAssetPosition { get; set; } = null!;
        #endregion

        /// <summary>
        /// Tổng hợp điểm đánh giá, 
        /// tạm thời là tổng của MacroeconomicsScore * 1, CompanyValueScore * 1, CompanyGrowthScore * 1, StockScore * 1. Tưởng lai có thể thay đổi các biến số để đánh trọng số cao hơn cho các score
        /// </summary>
        public int TotalScore => MacroeconomicsScore + CompanyValueScore + CompanyGrowthScore + StockScore;

        /// <summary>
        /// Lấy object dùng cho đẩy xuống client ở websocket
        /// </summary>
        /// <returns>dynamic</returns>
        public string GetSocketView()
        {
            return JsonSerializer.Serialize(new
            {
                smb = Symbol,
                clcp = LastClosePrice.ShowPrice(),
                LastOpenPrice,
                LastHighestPrice,
                LastLowestPrice,
                cepp = ExperimentProfitPercent.ShowPercent(),
                ceap = ExperimentAssetPosition,
                cmpp = MainProfitPercent.ShowPercent(),
                cmap = MainAssetPosition,
                capp = AccumulationProfitPercent.ShowPercent(),
                caap = AccumulationAssetPosition,
                cbpp = BuyAndHoldProfitPercent.ShowPercent(),
                cbap = BuyAndHoldAssetPosition,
            });
        }
    }
}