namespace Pl.Sas.Core.Entities
{
    /// <summary>
    /// Chỉ số tài chính của doanh nghiệp, dữ liệu của ssi
    /// </summary>
    public class FinancialIndicator : BaseEntity
    {
        /// <summary>
        /// Mã chứng khoán <see cref="Stock"/>
        /// </summary>
        public string Symbol { get; set; } = null!;

        /// <summary>
        /// năm báo cáo
        /// </summary>
        public int YearReport { get; set; }

        /// <summary>
        /// Quý báo cáo
        /// </summary>
        public int LengthReport { get; set; }

        /// <summary>
        /// Doanh thu
        /// </summary>
        public float Revenue { get; set; }

        /// <summary>
        /// Lợi nhuận
        /// </summary>
        public float Profit { get; set; }

        /// <summary>
        /// <para>Chỉ số EPS</para>
        /// <para>lợi nhuận (thu nhập) trên mỗi cổ phiếu.</para>
        /// <para>EPS = (Thu nhập ròng - cổ tức cổ phiếu ưu đãi) / lượng cổ phiếu bình quân đang lưu thông.</para>
        /// </summary>
        public float Eps { get; set; }

        /// <summary>
        /// <para>Chỉ số EPS pha loãng</para>
        /// <para>lợi nhuận (thu nhập) trên mỗi cổ phiếu.</para>
        /// <para>EPS = (Thu nhập ròng - cổ tức cổ phiếu ưu đãi) / lượng cổ phiếu bình quân đang lưu thông + các loại cổ phiếu, cổ tức khác có thể quy đổi sang cổ phiếu thường</para>
        /// </summary>
        public float DilutedEps { get; set; }

        /// <summary>
        /// <para>Chỉ số pe</para>
        /// <para>Dùng để đo lường mối quan hệ giữa Giá thị trường của cổ phiếu (Price) và Thu nhập trên một cổ phiếu (EPS).</para>
        /// <para>P/E = Giá thị trường của cổ phiếu (Price)/Thu nhập trên một cổ phiếu (EPS)</para>
        /// <para>Chỉ số P/E thể hiện mức giá mà nhà đầu tư sẵn sàng bỏ ra cho một đồng lợi nhuận thu được từ cổ phiếu.</para>
        /// </summary>
        public float Pe { get; set; }

        /// <summary>
        /// <para>Chỉ số pe</para>
        /// <para>Dùng để đo lường mối quan hệ giữa Giá thị trường của cổ phiếu (Price) và Thu nhập trên một cổ phiếu (DilutedEps).</para>
        /// <para>P/E = Giá thị trường của cổ phiếu (Price)/Thu nhập trên một cổ phiếu (DilutedEps)</para>
        /// <para>Chỉ số P/E thể hiện mức giá mà nhà đầu tư sẵn sàng bỏ ra cho một đồng lợi nhuận thu được từ cổ phiếu.</para>
        /// </summary>
        public float DilutedPe { get; set; }

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
        /// Chỉ số roic(Return on Invested Capital)
        /// <para>là một giá trị bằng số được sử dụng để đánh giá hiệu quả của một công ty trong việc phân bổ vốn dưới sự kiểm soát của công ty đó cho các khoản đầu tư sinh lời.</para>
        /// <para>https://vietnambiz.vn/he-so-thu-nhap-tren-von-dau-tu-return-on-invested-capital-roic-la-gi-20191105152810659.htm</para>
        /// </summary>
        public float Roic { get; set; }

        /// <summary>
        /// Tỷ suất lợi nhuận gộp
        /// <para>Tỉ suất lợi nhuận gộp  (%) = Lợi nhuận gộp / Doanh thu</para>
        /// <para>Lợi nhuận gộp = Doanh thu - Giá vốn hàng bán (COGS)</para>
        /// <para>là một chỉ số được sử dụng để đánh giá mô hình kinh doanh và sức khỏe tài chính của công ty bằng cách tiết lộ số tiền còn lại từ doanh thu sau khi trừ đi giá vốn hàng bán.</para>
        /// <para>https://vietnambiz.vn/ti-suat-loi-nhuan-gop-gross-profit-margin-la-gi-cong-thuc-xac-dinh-va-y-nghia-20191008164428741.htm</para>
        /// </summary>
        public float GrossProfitMargin { get; set; }

        /// <summary>
        /// <para>Biên lợi nhuận dòng</para>
        /// <para>Doanh nghiệp thu được bao nhiêu đồng lợi nhuận sau thuế, từ một đồng doanh thu.</para>
        /// <para>Biên lợi nhuận dòng = Lợi nhuận sau thuế / Doanh thu thuần</para>
        /// </summary>
        public float NetProfitMargin { get; set; }

        /// <summary>
        /// Tổng nợ / vốn chủ sở hữu (D/E)
        /// <para>Vốn chủ sở hữu là tất cả số vốn thuộc về cổ đông. Được cấu thành từ Vốn cổ phần (vốn điều lệ), Lợi nhuận chưa phân phối, và các nguồn khác.</para>
        /// <para>Hệ số này nhỏ hơn bình quân toàn ngành thì tốt toàn ngành thì tốt.</para>
        /// </summary>
        public float DebtEquity { get; set; }

        /// <summary>
        /// Tổng nợ / tài sản(D/A)
        /// <para>Tỉ số tổng nợ trên tổng tài sản là thước đo tài sản được tài trợ bằng nợ thay vì vốn chủ sở hữu của một công ty. </para>
        /// <para>Tỉ lệ TD/TA càng cao thì công ty có mức độ đòn bẩy (DoL) càng cao và do đó, rủi ro tài chính càng lớn. </para>
        /// </summary>
        public float DebtAsset { get; set; }

        /// <summary>
        /// Tỉ số thanh toán nhanh
        /// <para>Tỷ số thanh toán nhanh = (Tiền và các khoản tương đương tiền+các khoản phải thu+các khoản đầu tư ngắn hạn)/(Nợ ngắn hạn)</para>
        /// <para>Tỷ số thanh toán nhanh(quick ratio) cho biết liệu công ty có đủ các tài sản ngắn hạn để trả cho các khoản nợ ngắn hạn mà không cần phải bán hàng tồn kho hay không.</para>
        /// </summary>
        public float QuickRatio { get; set; }

        /// <summary>
        /// Tỉ lệ thanh toán hiện hành
        /// <para>Tỉ số thanh khoản hiện hành = Giá trị tải sản ngắn hạn / Giá trị nợ ngắn hạn</para>
        /// </summary>
        public float CurrentRatio { get; set; }

        /// <summary>
        /// Chỉ số pb
        /// <para>P/B = Giá cổ phiếu/Tổng giá trị tài sản – giá trị tài sản vô hình – nợ</para>
        /// <para>Chỉ số P/B (Price-to-Book ratio – Giá/Giá trị sổ sách) là tỷ lệ được sử dụng để so sánh giá của một cổ phiếu so với giá trị ghi sổ của cổ phiếu đó.</para>
        /// <para>https://vnexpress.net/chi-so-p-b-co-y-nghia-nhu-the-nao-2695409.html</para>
        /// </summary>
        public float Pb { get; set; }
    }
}