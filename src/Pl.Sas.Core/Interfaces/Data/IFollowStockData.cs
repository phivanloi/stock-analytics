using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Entities.Identity;

namespace Pl.Sas.Core.Interfaces
{
    public interface IFollowStockData
    {
        /// <summary>
        /// Lấy danh sách chứng khoán theo dõi của người dùng
        /// </summary>
        /// <param name="userId">id người dùng</param>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <returns>List FollowStock</returns>
        Task<List<FollowStock>> FindAllAsync(string userId, string? symbol = null);

        /// <summary>
        /// Lấy theo id
        /// </summary>
        /// <param name="id">Id cần lấy</param>
        /// <returns>FollowStock</returns>
        Task<FollowStock> FindByIdAsync(string id);

        /// <summary>
        /// Tìm một đối tượng theo id user và mã chứng khoán
        /// </summary>
        /// <param name="userId">Id user</param>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <returns>FollowStock</returns>
        Task<FollowStock> FindAsync(string userId, string symbol);

        /// <summary>
        /// Thêm mới một khoản đầu tư
        /// </summary>
        /// <param name="userInvest">Thông tin cần thêm mới</param>
        /// <returns>bool</returns>
        Task<bool> InsertAsync(FollowStock userStockNote);

        /// <summary>
        /// Xóa một khoản đầu tư
        /// </summary>
        /// <param name="id">Id khoản đầu tư</param>
        /// <returns>bool</returns>
        Task<bool> DeleteAsync(string id);

        /// <summary>
        /// Hàm kiểm tra xem người dùng có theo dõi mã chứng khoán nào không
        /// </summary>
        /// <param name="userId">Id người dùng</param>
        /// <returns>bool</returns>
        Task<bool> IsUserHasFollowAsync(string userId);
    }
}