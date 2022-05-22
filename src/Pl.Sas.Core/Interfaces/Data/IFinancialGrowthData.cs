using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    public interface IFinancialGrowthData
    {
        /// <summary>
        /// Lấy danh sách tăng trưởng tài chính
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <returns>IEnumerable FinancialGrowth</returns>
        Task<List<FinancialGrowth>> FindAllAsync(string symbol);

        /// <summary>
        /// Update một danh sách tăng trưởng tài chính
        /// </summary>
        /// <param name="financialGrowths">Danh sách cần sửa</param>
        /// <returns></returns>
        Task BulkUpdateAsync(IEnumerable<FinancialGrowth> financialGrowths);

        /// <summary>
        /// Thêm mới một danh sách tăng trưởng tài chính
        /// </summary>
        /// <param name="financialGrowths">Danh sách cần thêm mới</param>
        /// <returns></returns>
        Task BulkInserAsync(IEnumerable<FinancialGrowth> financialGrowths);

        /// <summary>
        /// xóa một bản ghi
        /// </summary>
        /// <param name="id">Id bản ghi cần xóa</param>
        /// <returns></returns>
        Task<bool> DeleteAsync(string id);

        /// <summary>
        /// Lấy giá trị cuối cùng của chỉ só đánh giá tăng trưởng của danh nghiệp
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <returns>FinancialGrowth</returns>
        Task<FinancialGrowth> GetLastAsync(string symbol);
    }
}