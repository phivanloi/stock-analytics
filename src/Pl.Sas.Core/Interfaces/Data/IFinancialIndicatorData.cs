using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    public interface IFinancialIndicatorData
    {
        /// <summary>
        /// Lấy danh sách 5 năm dữ liệu tài chính của các mã chứng khoán cùng ngành
        /// </summary>
        /// <param name="industryCode">Mã ngành</param>
        /// <param name="symbols">Các mã chứng khoán</param>
        /// <param name="yearRanger">Số năm báo cáo cần lấy</param>
        /// <returns></returns>
        Task<List<FinancialIndicator>?> CacheGetBySymbolsAsync(string industryCode, string[] symbols, int yearRanger = 5);

        /// <summary>
        /// Lấy danh sách chỉ số tài chính theo mã cổ phiếu
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <param name="yearReport">Năm báo cáo</param>
        /// <param name="lengthReport">Quý báo cáo, với lengthReport = 5 là trạng thái báo cáo cả năm</param>
        /// <returns>IEnumerable FinancialIndicator</returns>
        Task<IEnumerable<FinancialIndicator>> FindAllAsync(string symbol, int? yearReport = null, int? lengthReport = null);

        /// <summary>
        /// Lấy top dữ liệu báo cáo tài chính mới nhất, năm và quý báo cáo giảm dần
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <param name="top">Số báo cáo cần lấy</param>
        /// <returns>List FinancialIndicator</returns>
        Task<List<FinancialIndicator>> GetTopAsync(string symbol, int top = 10);

        /// <summary>
        /// Sửa danh sách mã cổ phiếu
        /// </summary>
        /// <param name="financialIndicators">Danh sách chỉ số tài chính</param>
        /// <returns>Task</returns>
        Task BulkUpdateAsync(IEnumerable<FinancialIndicator> financialIndicators);

        /// <summary>
        /// Thêm mới danh sách mã cổ phiếu
        /// </summary>
        /// <param name="financialGrowths">Danh sách chỉ số tài chính</param>
        /// <returns>Task</returns>
        Task BulkInserAsync(IEnumerable<FinancialIndicator> financialGrowths);

        /// <summary>
        /// Xóa một chỉ số tài chính
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> DeleteAsync(string id);
    }
}