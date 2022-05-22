using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    public interface ILeadershipData
    {
        /// <summary>
        /// Thêm mới một danh sách lãnh đạo của công ty
        /// </summary>
        /// <param name="leaderships">Danh sách </param>
        /// <returns></returns>
        Task BulkInserAsync(List<Leadership> leaderships);

        /// <summary>
        /// Sửa danh sách lãnh đạo
        /// </summary>
        /// <param name="leaderships">Danh sách lãnh đạo</param>
        /// <returns>List Leadership</returns>
        Task BulkUpdateAsync(List<Leadership> leaderships);

        /// <summary>
        /// Lấy danh sách
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <param name="actived">Trạng thái active</param>
        /// <returns>IReadOnlyList Leadership</returns>
        Task<IReadOnlyList<Leadership>> FindAllAsync(string symbol, bool? actived = null);

        Task<bool> DeleteAsync(string id);
    }
}