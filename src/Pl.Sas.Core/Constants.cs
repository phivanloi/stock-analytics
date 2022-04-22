namespace Pl.Sas.Core
{
    public static class Constants
    {
        /// <summary>
        /// Bộ tên tất cả các chỉ số chính của sản chứng khoán
        /// </summary>
        public static readonly string[] ShareIndex = new string[] { "VNINDEX", "VN30", "HNX", "HNX30", "UPCOM" };

        /// <summary>
        /// ngày này được mặc định dùng để làm mốc cho việc trading thử nghiệm
        /// </summary>
        public static readonly DateTime StartTime = new(2020, 1, 1);
    }
}
