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

        #region Company Value
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
        #endregion

        #region Cột % lợi nhận sau thuế quý gần nhất và % tăng trưởng 2 năm
        public string Lnqvn { get; set; } = null!;
        public string LnqvnCss { get; set; } = "lnqvn t-r";
        #endregion        

        #region Cột % doanh thu quý gần nhất và  % tăng trưởng 2 năm
        public string Dtqvn { get; set; } = null!;
        public string DtqvnCss { get; set; } = "dtqvn t-r";
        #endregion

        #region Cột điểm số doanh nghiệp
        public string Score { get; set; } = null!;
        public string ScoreCss { get; set; } = "score t-r";
        public float ScoreValue { get; set; } = 0;
        #endregion

        #region Cột dòng tiền theo ngành
        public string Icf { get; set; } = null!;
        public string IcfCss { get; set; } = "icf t-r";
        #endregion

        #region Cột dòng tiền của cổ phiếu
        public string Scf { get; set; } = null!;
        public string ScfCss { get; set; } = "scf t-r";
        #endregion

        #region Cột chỉ số beta
        public string Beta { get; set; } = null!;
        public string BetaCss { get; set; } = "beta t-r";
        #endregion

        #region Cột Sức mạnh giá stoch rsi 14
        public string Rsi14 { get; set; } = null!;
        public string Rsi14Css { get; set; } = "rsi14 t-r";
        #endregion

        #region Cột giá hỗ trợ
        public string Nght { get; set; } = null!;
        public string NghtCss { get; set; } = "nght t-r";
        #endregion

        #region Cột giá kháng cự
        public string Ngkc { get; set; } = null!;
        public string NgkcCss { get; set; } = "ngkc t-r";
        #endregion

        #region Cột khối lượng hiện tại
        public float KlhtValue { get; set; }
        public string Klht { get; set; } = null!;
        public string KlhtCss { get; set; } = "klht t-r";
        #endregion

        #region Cột khối lượng phiên trước
        public float KlptValue { get; set; }
        #endregion

        #region Cột giá hiện tại
        public string Ght { get; set; } = null!;
        public string GhtCss { get; set; } = "ght t-r";
        #endregion

        #region Cột % biến động giá 2 phiên tăng giảm so với phiên trước
        public float Bd2Value { get; set; }
        public string Bd2 { get; set; } = null!;
        public string Bd2Css { get; set; } = "bd2 t-r";
        #endregion

        #region Cột % biến động giá 5 phiên
        public string Bd5 { get; set; } = null!;
        public string Bd5Css { get; set; } = "bd5 t-r";
        #endregion

        #region Cột % biến động giá 10 phiên
        public string Bd10 { get; set; } = null!;
        public string Bd10Css { get; set; } = "bd10 t-r";
        #endregion

        #region Cột % biến động giá 30 phiên
        public string Bd30 { get; set; } = null!;
        public string Bd30Css { get; set; } = "bd30 t-r";
        #endregion

        #region Cột % biến động giá 60 phiên
        public string Bd60 { get; set; } = null!;
        public string Bd60Css { get; set; } = "bd60 t-r";
        #endregion

        #region Cột % Lợi nhận phương pháp thử nghiệm
        public string Lntn { get; set; } = null!;
        public string LntnCss { get; set; } = "lntn t-r";
        #endregion

        #region Cột % Lợi nhận phương pháp chính
        public string Lnc { get; set; } = null!;
        public string LncCss { get; set; } = "lnc t-r";
        #endregion

        #region Cột % Lợi nhận mua và giữ
        public string Lnmg { get; set; } = null!;
        public string LnmgCss { get; set; } = "lnmg t-r";
        #endregion

        #region Cột Khuyến nghị theo phương pháp thử nghiệm
        public string Kntn { get; set; } = null!;
        public string KntnCss { get; set; } = "kntn t-c";
        #endregion

        #region Cột Khuyến nghị theo phương pháp chính
        public string Knc { get; set; } = null!;
        public string KncCss { get; set; } = "knc t-c";
        #endregion
    }
}