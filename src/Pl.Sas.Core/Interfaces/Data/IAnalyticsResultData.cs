using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    public interface IAnalyticsResultData
    {
        /// <summary>
        /// Thêm mới một danh sách kết quả phân tích
        /// </summary>
        /// <param name="analyticsResults">Danh sách kết quả phân tích</param>
        /// <returns></returns>
        Task BulkInserAsync(IEnumerable<AnalyticsResult> analyticsResults);

        /// <summary>
        /// Lấy dữ liệu phân tích
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <returns>AnalyticsResult</returns>
        Task<AnalyticsResult> FindAsync(string symbol);

        /// <summary>
        /// Ghi lại kết quả đánh giá kinh tế vĩ mô
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <param name="marketScore">Điểm đánh giá</param>
        /// <param name="marketNotes">Ghi chú đánh giá</param>
        /// <returns>bool</returns>
        Task<bool> SaveMacroeconomicsScoreAsync(string symbol, int marketScore, byte[] marketNotes);

        /// <summary>
        /// Ghi lại kết quả đánh giá giá trị doanh nghiệp
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <param name="companyValueScore">Điểm đánh giá</param>
        /// <param name="companyValueNotes">Ghi chú đánh giá</param>
        /// <returns>bool</returns>
        Task<bool> SaveCompanyValueScoreAsync(string symbol, int companyValueScore, byte[] companyValueNotes);

        /// <summary>
        /// Ghi lại kết quả đánh giá tăng trưởng doanh nghiệp
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <param name="companyGrowthScore">Điểm đánh giá</param>
        /// <param name="companyGrowthNotes">Ghi chú đánh giá</param>
        /// <returns>bool</returns>
        Task<bool> SaveCompanyGrowthScoreAsync(string symbol, int companyGrowthScore, byte[] companyGrowthNotes);

        /// <summary>
        /// Ghi lại kết quả đánh giá diao dịch thị trường
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <param name="stockScore">Điểm đánh giá</param>
        /// <param name="stockNotes">Ghi chú đánh giá</param>
        /// <returns>bool</returns>
        Task<bool> SaveStockScoreAsync(string symbol, int stockScore, byte[] stockNotes);

        /// <summary>
        /// Ghi lại kết quả đánh giá của fiintrading.vn
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <param name="fiinScore">Điểm đánh giá</param>
        /// <param name="fiinNotes">Ghi chú đánh giá</param>
        /// <returns>bool</returns>
        Task<bool> SaveFiinScoreAsync(string symbol, int fiinScore, byte[] fiinNotes);

        /// <summary>
        /// Ghi lại kết quả đánh giá của vndirect
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <param name="vndScore">Điểm đánh giá</param>
        /// <param name="vndNotes">Ghi chú đánh giá</param>
        /// <returns>bool</returns>
        Task<bool> SaveVndScoreAsync(string symbol, int vndScore, byte[] vndNotes);

        /// <summary>
        /// Ghi lại kết quả dự phóng giá cổ phiếu
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <param name="targetPrice">Giá dự phóng trung bình</param>
        /// <param name="targetPriceNotes">Ghi chú tính giá</param>
        /// <returns></returns>
        Task<bool> SaveTargetPriceAsync(string symbol, float targetPrice, byte[] targetPriceNotes);
    }
}