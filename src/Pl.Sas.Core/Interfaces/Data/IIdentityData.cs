namespace Pl.Sas.Core.Interfaces
{
    /// <summary>
    /// Lớp xử lỹ liệu của người dùng
    /// </summary>
    public interface IIdentityData
    {
        /// <summary>
        /// Kiểm tra xem user có đang theo dõi cố phiếu nào không
        /// </summary>
        /// <param name="userId">Id user</param>
        /// <returns>bool</returns>
        Task<bool> IsUserHasFollowAsync(string userId);

        /// <summary>
        /// Lấy danh sách theo dõi cố phiếu
        /// </summary>
        /// <param name="userId">Id người dùng</param>
        /// <returns>List string</returns>
        Task<List<string>> GetFollowSymbols(string userId);

        /// <summary>
        /// Thực hiện thêm mới hoặc gỡ bỏ theo dõi đã có
        /// </summary>
        /// <param name="userId">Id user</param>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <returns>bool</returns>
        Task<bool> ToggleFollow(string userId, string symbol);
    }
}
