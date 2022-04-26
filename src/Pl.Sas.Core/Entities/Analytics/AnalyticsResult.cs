using System;

namespace Pl.Sas.Core.Entities
{
    public class AnalyticsResult : BaseEntity
    {
        /// <summary>
        /// Mã chứng khoán
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Ngày phân tích
        /// </summary>
        public DateTime TradingDate { get; set; } = Utilities.GetTradingDate();

        /// <summary>
        /// Chuỗi đại diện cho ngày phân tích
        /// </summary>
        public string DatePath { get; set; } = Utilities.GetTradingDatePath();

        /// <summary>
        /// Giá dự đoán bằng thuật toán ssa
        /// </summary>
        public decimal SsaPerdictPrice { get; set; } = -1;

        /// <summary>
        /// Giá dự đoán bằng thuật toán Ftt
        /// </summary>
        public decimal FttPerdictPrice { get; set; } = -1;

        /// <summary>
        /// Dự báo su thế theo thuật toán Sdca. Hiện tại chưa dùng do kết quả không tốt
        /// 0 là giảm, 1 là giữ giá, 2 là tăng giá
        /// </summary>
        public int SdcaPriceTrend { get; set; } = -1;

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
        public int MacroeconomicsScore { get; set; } = -100;

        /// <summary>
        /// Ghi chú đánh giá vĩ mô, thể chế, trạng thái kinh tế
        /// </summary>
        public byte[] MacroeconomicsNote { get; set; } = null;

        /// <summary>
        /// Điểm đánh giá giá trị của doanh nghiệp
        /// <para></para>
        /// </summary>
        public int CompanyValueScore { get; set; } = -100;

        /// <summary>
        /// Ghi chú đánh giá giá trị của doanh nghiệp
        /// </summary>
        public byte[] CompanyValueNote { get; set; } = null;

        /// <summary>
        /// Điểm đánh giá tăng trưởng doanh nghiệp
        /// <para></para>
        /// </summary>
        public int CompanyGrowthScore { get; set; } = -100;

        /// <summary>
        /// Ghi chú đánh giá tăng trưởng của doanh nghiệp
        /// </summary>
        public byte[] CompanyGrowthNote { get; set; } = null;

        /// <summary>
        /// Điểm đánh giá biến động thị giá
        /// </summary>
        public int StockScore { get; set; } = -100;

        /// <summary>
        /// Ghi chú đánh giá giao dịch của chứng khoán
        /// </summary>
        public byte[] StockNote { get; set; } = null;

        /// <summary>
        /// Điểm đánh giá của fiin
        /// </summary>
        public int FiinScore { get; set; } = -100;

        /// <summary>
        /// Nội dung đánh giá của fiin
        /// </summary>
        public byte[] FiinNote { get; set; } = null;

        /// <summary>
        /// Điểm đánh giá của vnd
        /// </summary>
        public int VndScore { get; set; } = -100;

        /// <summary>
        /// Nội dung đánh giá của vnd
        /// </summary>
        public byte[] VndNote { get; set; } = null;

        /// <summary>
        /// Giá dự phóng trung bình của các công ty chứng khoán
        /// </summary>
        public decimal TargetPrice { get; set; } = -1;

        /// <summary>
        /// Ghi chú giá dự phong trung bình của các công ty chứng khoán
        /// </summary>
        public byte[] TargetPriceNotes { get; set; } = null;

        /// <summary>
        /// Tổng hợp điểm đánh giá, 
        /// tạm thời là tổng của MacroeconomicsScore * 1, CompanyValueScore * 1, CompanyGrowthScore * 1, StockScore * 1. Tưởng lai có thể thay đổi các biến số để đánh trọng số cao hơn cho các score
        /// </summary>
        public int TotalScore => MacroeconomicsScore + CompanyValueScore + CompanyGrowthScore + StockScore;
    }
}