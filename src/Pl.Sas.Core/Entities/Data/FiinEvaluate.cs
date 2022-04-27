namespace Pl.Sas.Core.Entities
{
    /// <summary>
    /// Lưu đánh giá cổ phiếu của công ty fiin
    /// </summary>
    public class FiinEvaluated : BaseEntity
    {
        /// <summary>
        /// Mã cổ phiếu
        /// </summary>
        public string Symbol { get; set; } = null!;

        /// <summary>
        /// Đánh giá xếp hạng trong ngành
        /// -1 là không được sếp hàng
        /// </summary>
        public int IcbRank { get; set; } = -1;

        /// <summary>
        /// Tổng số rank trong ngành tiêu chuẩn
        /// -1 là không có thông tin ngành 
        /// </summary>
        public int IcbTotalRanked { get; set; } = -1;

        /// <summary>
        /// Đánh giá sếp hạng trong rổ cổ phiếu
        /// -1 là không có thông tin trong rổ cổ phiếu nào
        /// </summary>
        public int IndexRank { get; set; } = -1;

        /// <summary>
        /// Tổng số rank trong rổ cổ phiếu
        /// -1 là không có thông tin rổ cổ phiếu
        /// </summary>
        public int IndexTotalRanked { get; set; } = -1;

        /// <summary>
        /// Mã ngành chuản của cô phiếu
        /// </summary>
        public string? IcbCode { get; set; }

        /// <summary>
        /// Mã rổ cổ phiếu
        /// </summary>
        public string? ComGroupCode { get; set; }

        /// <summary>
        /// Điểm đánh giá tăng trưởng doanh nghiệp thang điểm A,B,C,D,F với A là cao nhất
        /// </summary>
        public string? Growth { get; set; }

        /// <summary>
        /// Điểm đánh giá giá trị của doanh nghiệp thang điểm A,B,C,D,F với A là cao nhất
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Điểm đánh giá về tăng tưởng thị giá với cổ phiếu thang điểm A,B,C,D,F với A là cao nhất
        /// </summary>
        public string? Momentum { get; set; }

        /// <summary>
        /// Điểm đánh giá trung bình của tổng hợp 3 tiêu chí (Growth, Value, Momentum) thang điểm A,B,C,D,F với A là cao nhất
        /// </summary>
        public string? Vgm { get; set; }

        /// <summary>
        /// Trạng thái của danh nghiệp trên sở giao dịch chứng khoán
        /// </summary>
        public int ControlStatusCode { get; set; } = 0;

        /// <summary>
        /// Nội dung của trạng thái
        /// </summary>
        public string? ControlStatusName { get; set; }
    }
}