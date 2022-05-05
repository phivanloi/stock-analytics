namespace Pl.Sas.Core
{
    public static class Constants
    {
        /// <summary>
        /// Thời gian cache mặc định tính theo giây
        /// </summary>
        public const int DefaultCacheTime = 60;

        /// <summary>
        /// ngày này được mặc định dùng để làm mốc cho việc trading thử nghiệm
        /// </summary>
        public static readonly DateTime StartTime = new(2020, 1, 1);

        /// <summary>
        /// Khóa cache cho schedule
        /// </summary>
        public const string ScheduleCachePrefix = "SCHDUCPF";

        /// <summary>
        /// Khóa cache cho FinancialIndicator
        /// </summary>
        public const string FinancialIndicatorCachePrefix = "FIICPF";

        /// <summary>
        /// Khóa cache cho company
        /// </summary>
        public const string CompanyCachePrefix = "COMCPF";

        /// <summary>
        /// Khóa cache cho Stock
        /// </summary>
        public const string StockCachePrefix = "STOCCPF";

        #region Lãi suất ngân hàng
        /// <summary>
        /// Lái suất ngần hàng kỳ hạn 3 tháng. Lấy ngân hàng có mức lái suất cao nhất
        /// </summary>
        public static readonly string BankInterestRate3Key = "BankInterestRate3";

        /// <summary>
        /// Lái suất ngần hàng kỳ hạn 6 tháng. Lấy ngân hàng có mức lái suất cao nhất
        /// </summary>
        public static readonly string BankInterestRate6Key = "BankInterestRate6";

        /// <summary>
        /// Lái suất ngần hàng kỳ hạn 12 tháng. Lấy ngân hàng có mức lái suất cao nhất
        /// </summary>
        public static readonly string BankInterestRate12Key = "BankInterestRate12";

        /// <summary>
        /// Lái suất ngần hàng kỳ hạn 24 tháng. Lấy ngân hàng có mức lái suất cao nhất
        /// </summary>
        public static readonly string BankInterestRate24Key = "BankInterestRate24";
        #endregion
    }
}
