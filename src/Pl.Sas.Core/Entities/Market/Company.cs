namespace Pl.Sas.Core.Entities
{
    /// <summary>
    /// Công ty
    /// </summary>
    public class Company : BaseEntity
    {
        /// <summary>
        /// Mã chứng khoán <see cref="Stock"/>
        /// </summary>
        public string Symbol { get; set; } = null!;

        /// <summary>
        /// Mã ngành sic
        /// </summary>
        public string SubsectorCode { get; set; } = null!;

        /// <summary>
        /// Tên ngành tra cứu
        /// </summary>
        public string? IndustryName { get; set; }

        /// <summary>
        /// Tên ngành hoạt động chính
        /// </summary>
        public string? Supersector { get; set; }

        /// <summary>
        /// Mảng hoạt động
        /// </summary>
        public string? Sector { get; set; }

        /// <summary>
        /// Tên ngành phụ
        /// </summary>
        public string Subsector { get; set; } = null!;

        /// <summary>
        /// Ngày thành lập
        /// </summary>
        public DateTime? FoundingDate { get; set; } = null;

        /// <summary>
        /// ngày niêm yết trên sàn chứng khoán
        /// </summary>
        public DateTime? ListingDate { get; set; } = null;

        /// <summary>
        /// Vốn điều lệ
        /// </summary>
        public float CharterCapital { get; set; }

        /// <summary>
        /// Số lượng nhân viên
        /// </summary>
        public int NumberOfEmployee { get; set; }

        /// <summary>
        /// số chi nhánh
        /// </summary>
        public int BankNumberOfBranch { get; set; }

        /// <summary>
        /// Giới thiệu công ty được nén lại
        /// </summary>
        public byte[]? CompanyProfile { get; set; } = null;

        /// <summary>
        /// Sàn niêm yết
        /// </summary>
        public string? Exchange { get; set; }

        /// <summary>
        /// Giá chào sàn
        /// </summary>
        public float FirstPrice { get; set; }

        /// <summary>
        /// Khối lượng cổ phiếu đang niêm yết
        /// </summary>
        public float IssueShare { get; set; }

        /// <summary>
        /// Thị giá vốn
        /// </summary>
        public float ListedValue { get; set; }

        /// <summary>
        /// Tên công ty
        /// </summary>
        public string? CompanyName { get; set; }

        #region Statistics

        /// <summary>
        /// <para>Vốn hóa thị trường</para>
        /// <para>là tổng giá trị của số cổ phần của một công ty niêm yết</para>
        /// <para>Vốn hóa = Giá 1 cổ phiếu X số lượng cổ phiếu đang lưu hành.</para>
        /// </summary>
        public float MarketCap { get; set; }

        /// <summary>
        /// Số cổ phiếu phát hành ra ngoài
        /// </summary>
        public float SharesOutStanding { get; set; }

        /// <summary>
        /// <para>Chỉ số BV (Giá sổ sách)</para>
        /// <para>Vốn CSH – Tài sản vô hình/Tổng khối lượng cổ phiếu đang lưu hành</para>
        /// </summary>
        public float Bv { get; set; }

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
        /// Chỉ số pb
        /// <para>P/B = Giá cổ phiếu/Tổng giá trị tài sản – giá trị tài sản vô hình – nợ</para>
        /// <para>Chỉ số P/B (Price-to-Book ratio – Giá/Giá trị sổ sách) là tỷ lệ được sử dụng để so sánh giá của một cổ phiếu so với giá trị ghi sổ của cổ phiếu đó.</para>
        /// <para>https://vnexpress.net/chi-so-p-b-co-y-nghia-nhu-the-nao-2695409.html</para>
        /// </summary>
        public float Pb { get; set; }

        /// <summary>
        /// Tỉ suất cổ tức
        /// là lệ % mà cổ đông nhận được trên mỗi cổ phân
        /// DividendYield = giá cổ tức/giá cổ phiếu hiện hành
        /// </summary>
        public float DividendYield { get; set; }

        /// <summary>
        /// Tổng doanh thu của năm tài chính trước đó
        /// </summary>
        public float TotalRevenue { get; set; }

        /// <summary>
        /// Lợi nhuận sau thuế của năm tài chính trước đó
        /// </summary>
        public float Profit { get; set; }

        /// <summary>
        /// Tài sản
        /// </summary>
        public float Asset { get; set; }

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
        /// <para>Chỉ số npl</para>
        /// <para>tỉ lệ nợ sấu của các doanh nghiệp tài chính
        /// Tỉ lệ nợ xấu = (Dư nợ nợ xấu/Tổng dư nợ) x 100%.
        /// </para>
        /// </summary>
        public float Npl { get; set; }

        /// <summary>
        /// Đòn bẩy tài chính là mức đòn bẩy mà ssi cung cho công ty này
        /// </summary>
        public float FinanciallEverage { get; set; }

        #endregion
    }
}