using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    public interface ICompanyData
    {
        /// <summary>
        /// Thêm mới một danh sách công ty niêm yết
        /// </summary>
        /// <param name="companies">Danh sách công ty cần thêm mới</param>
        /// <returns></returns>
        Task BulkInserAsync(List<Company> companies);

        /// <summary>
        /// Update một danh sách công ty niêm yết
        /// </summary>
        /// <param name="companies">Danh sách công ty cần update</param>
        /// <returns></returns>
        Task BulkUpdateAsync(List<Company> companies);

        /// <summary>
        /// lấy thông tin doanh nghiệp
        /// </summary>
        /// <param name="subsectorCode">Mã mã ngành</param>
        /// <returns></returns>
        Task<List<Company>> FindAllAsync(string? subsectorCode = null);

        /// <summary>
        /// Delete company by id
        /// </summary>
        /// <param name="id">id to delete</param>
        /// <returns>bool</returns>
        Task<bool> DeleteAsync(string id);

        /// <summary>
        /// Thêm mới một doanh nghiệp
        /// </summary>
        /// <param name="company">Danh nghiệp cần thêm mới</param>
        /// <returns>bool</returns>
        Task<bool> InsertAsync(Company company);

        /// <summary>
        /// Sửa một doanh nghiệp
        /// </summary>
        /// <param name="company">Doanh nghiệp cần sửa</param>
        /// <returns>bool</returns>
        Task<bool> UpdateAsync(Company company);

        /// <summary>
        /// Lấy thông tin công ty theo mã cổ phiếu
        /// </summary>
        /// <param name="symbol">Mã cô rphieeus</param>
        /// <returns>Company</returns>
        Task<Company> FindBySymbolAsync(string symbol);

        /// <summary>
        /// Lấy toàn bộ công ty trong hệ thống để phân tích
        /// </summary>
        /// <param name="subsectorCode">Mã mã ngành</param>
        /// <returns>IEnumerable Company</returns>
        Task<List<Company>?> CacheFindAllForAnalyticsAsync(string? subsectorCode = null);
    }
}