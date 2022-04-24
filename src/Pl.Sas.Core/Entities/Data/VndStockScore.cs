namespace Pl.Sas.Core.Entities
{
    /// <summary>
    /// Lưu thông tin đánh giá cổ phiếu của vnd
    /// </summary>
    public class VndStockScore : BaseEntity
    {
        /// <summary>
        /// Mã cổ phiếu
        /// </summary>
        public string Symbol { get; set; } = null!;

        /// <summary>
        /// Loại stock
        /// </summary>
        public string Type { get; set; } = null!;

        /// <summary>
        /// Ngày đánh giá
        /// </summary>
        public DateTime FiscalDate { get; set; }

        /// <summary>
        /// Mã đánh giá của Vnd
        /// </summary>
        public string ModelCode { get; set; } = null!;

        /// <summary>
        /// Mã tiêu chuẩn đánh giá của vnd
        /// <para>100000 => Đánh giá doanh nghiệp theo Stock Rating Model</para>
        /// <para>101000 => Nhóm tiêu chí đánh giá vị thế doanh nghiệp</para>
        /// <para>102000 => Nhóm tiêu chí đánh giá tốc độ tăng trưởng</para>
        /// <para>103000 => Năng lực sinh lời của doanh nghiệp</para>
        /// <para>104000 => Cam kết với cổ đông</para>
        /// <para>105000 => Rủi ro tài chính của doanh nghiệp</para>
        /// </summary>
        public string CriteriaCode { get; set; } = null!;

        /// <summary>
        /// Loại tiêu chuẩn đánh giá
        /// </summary>
        public string CriteriaType { get; set; } = null!;

        /// <summary>
        /// Tên tiêu chuẩn đánh giá
        /// </summary>
        public string? CriteriaName { get; set; }

        /// <summary>
        /// điểm đánh giá, Thang điểm từ 1 đến 10
        /// </summary>
        public float Point { get; set; }

        /// <summary>
        /// Vị trí doanh nghiệp đánh giá
        /// </summary>
        public string? Locale { get; set; }
    }
}