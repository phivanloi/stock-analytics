namespace Pl.Sas.Core.Entities
{
    /// <summary>
    /// Kết quả phân tích ngành nghề
    /// </summary>
    public class IndustryAnalytics : BaseEntity
    {
        /// <summary>
        /// Điểm đánh giá thủ công
        /// </summary>
        public float ManualScore { get; set; }

        /// <summary>
        /// Điểm phân tích
        /// </summary>
        public float Score { get; set; }

        /// <summary>
        /// Ghi chú đánh giá dòng tiền theo ngành <see cref="AnalyticsNote"/>
        /// </summary>
        public byte[]? Notes { get; set; } = null;

        /// <summary>
        /// Mã ngành <see cref="Industry"/>
        /// </summary>
        public string Code { get; set; } = null!;
    }
}