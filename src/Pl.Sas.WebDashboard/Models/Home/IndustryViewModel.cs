namespace Pl.Sas.WebDashboard.Models
{
    public class IndustryViewModel
    {
        /// <summary>
        /// Tên loại hình công ty
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Mã ngành bên ssi crawler về
        /// </summary>
        public string Code { get; set; } = null!;

        /// <summary>
        /// Điểm đánh giá thủ công
        /// </summary>
        public float ManualScore { get; set; }

        /// <summary>
        /// Điểm phân tích
        /// </summary>
        public float Score { get; set; }
    }
}