using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    /// <summary>
    /// Nghiệp vụ ngành
    /// </summary>
    public interface IIndustryData
    {
        /// <summary>
        /// Thêm mới một danh sách ngành hoạt động chính của doanh nghiệp
        /// </summary>
        /// <param name="industries">Danh sách ngành cần thêm</param>
        /// <returns>void</returns>
        Task BulkInserAsync(List<Industry> industries);

        /// <summary>
        /// Lấy ra danh sách ngành hoạt động
        /// </summary>
        /// <returns>IEnumerable Industry</returns>
        Task<IEnumerable<Industry>> FindAllAsync();

        /// <summary>
        /// Xóa một ngành
        /// </summary>
        /// <param name="id">Id cần xóa</param>
        /// <returns></returns>
        Task<bool> DeleteAsync(string id);

        /// <summary>
        /// Lấy id ngành theo tên ngành
        /// </summary>
        /// <param name="name">Tên ngành cần lấy</param>
        /// <returns>string</returns>
        Task<string> GetIdByNameAsync(string name);

        /// <summary>
        /// Lấy theo id
        /// </summary>
        /// <param name="id">Id cần lấy</param>
        /// <returns>Industry</returns>
        Task<Industry> GetByIdAsync(string id);

        /// <summary>
        /// Lấy thông tin ngành theo mã
        /// </summary>
        /// <param name="code">mã ngành</param>
        /// <returns></returns>
        Task<Industry> GetByCodeAsync(string code);

        /// <summary>
        /// Thêm mới một ngành vào hệ thống
        /// </summary>
        /// <param name="industry">Ngành cần thêm mới</param>
        /// <returns>bool</returns>
        Task<bool> InsertAsync(Industry industry);

        /// <summary>
        /// Sửa một ngành mới
        /// </summary>
        /// <param name="industry">Ngành cần sửa</param>
        /// <returns>bool</returns>
        Task<bool> UpdateAsync(Industry industry);
    }
}