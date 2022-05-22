using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    /// <summary>
    /// Lớp xử lỹ liệu của phân tích
    /// </summary>
    public interface IAnalyticsData
    {
        /// <summary>
        /// Ghi lại điểm đánh giá
        /// </summary>
        /// <param name="code">Mã ngành</param>
        /// <param name="manualScore">điểm đánh giá</param>
        /// <returns>bool</returns>
        Task<bool> SaveManualScoreAsync(string code, float manualScore);

        /// <summary>
        /// Lấy danh sách trading thử nghiệm
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <returns>TradingResult</returns>
        Task<List<TradingResult>> GetTradingResultAsync(string symbol);

        /// <summary>
        /// Lấy toàn bộ phân tích ngành
        /// </summary>
        /// <returns></returns>
        Task<List<IndustryAnalytics>> GetIndustryAnalyticsAsync();

        /// <summary>
        /// Ghi lại kết quả trading thử nghiệm
        /// </summary>
        /// <param name="tradingResult">Kết quả tradinh thử nghiệm</param>
        /// <returns>bool</returns>
        Task<bool> SaveTestTradingResultAsync(TradingResult tradingResult);

        /// <summary>
        /// Lấy phân tích dòng tiền theo ngành
        /// </summary>
        /// <param name="code">Mã</param>
        /// <returns>IndustryAnalytics</returns>
        Task<IndustryAnalytics?> GetIndustryAnalyticsAsync(string code);

        /// <summary>
        /// Ghi kết quả phân tích dòng tiền theo ngành
        /// </summary>
        /// <param name="code">Mã ngành</param>
        /// <param name="score">điểm đánh giá</param>
        /// <param name="analyticsNote">Ghi chú đánh giá</param>
        /// <returns>bool</returns>
        Task<bool> SaveIndustryScoreAsync(string code, float score, byte[] analyticsNote);

        /// <summary>
        /// Ghi lại kết quả đánh giá giá trị doanh nghiệp
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <param name="marketScore">Điểm đánh giá</param>
        /// <param name="marketNote">Ghi chú đánh giá</param>
        /// <returns>bool</returns>
        Task<bool> SaveCompanyValueScoreAsync(string symbol, int marketScore, byte[] marketNote);

        /// <summary>
        /// Ghi lại kết quả đánh giá kinh tế vĩ mô
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <param name="macroeconomicsScore">Điểm đánh giá</param>
        /// <param name="macroeconomicsNote">Ghi chú đánh giá</param>
        /// <returns>bool</returns>
        Task<bool> SaveMacroeconomicsScoreAsync(string symbol, int macroeconomicsScore, byte[] macroeconomicsNote);

        /// <summary>
        /// Ghi lại kết quả đánh giá tăng trưởng doanh nghiệp
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <param name="companyGrowthScore">Điểm đánh giá</param>
        /// <param name="companyGrowthNote">Ghi chú đánh giá</param>
        /// <returns>bool</returns>
        Task<bool> SaveCompanyGrowthScoreAsync(string symbol, int companyGrowthScore, byte[] companyGrowthNote);

        /// <summary>
        /// Ghi lại kết quả đánh giá diao dịch thị trường
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <param name="stockScore">Điểm đánh giá</param>
        /// <param name="stockNote">Ghi chú đánh giá</param>
        /// <returns>bool</returns>
        Task<bool> SaveStockScoreAsync(string symbol, int stockScore, byte[] stockNote);

        /// <summary>
        /// Ghi lại kết quả đánh giá của fiintrading.vn
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <param name="fiinScore">Điểm đánh giá</param>
        /// <param name="fiinNote">Ghi chú đánh giá</param>
        /// <returns>bool</returns>
        Task<bool> SaveFiinScoreAsync(string symbol, int fiinScore, byte[] fiinNote);

        /// <summary>
        /// Ghi lại kết quả đánh giá của vndirect
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <param name="vndScore">Điểm đánh giá</param>
        /// <param name="vndNote">Ghi chú đánh giá</param>
        /// <returns>bool</returns>
        Task<bool> SaveVndScoreAsync(string symbol, int vndScore, byte[] vndNote);

        /// <summary>
        /// Ghi lại kết quả dự phóng giá cổ phiếu
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <param name="targetPrice">Giá dự phóng trung bình</param>
        /// <param name="targetPriceNote">Ghi chú tính giá</param>
        /// <returns>bool</returns>
        Task<bool> SaveTargetPriceAsync(string symbol, float targetPrice, byte[] targetPriceNote);

        /// <summary>
        /// Lấy thông tin phân tích theo mã chứng khoán và ngày
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <returns>AnalyticsResult</returns>
        Task<AnalyticsResult?> GetAnalyticsResultAsync(string symbol);
    }
}
