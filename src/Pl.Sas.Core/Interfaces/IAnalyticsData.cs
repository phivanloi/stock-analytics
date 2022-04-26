using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    /// <summary>
    /// Lớp xử lỹ liệu của phân tích
    /// </summary>
    public interface IAnalyticsData
    {
        /// <summary>
        /// Thêm mới danh sách theo dõi xử lý cổ phiếu
        /// </summary>
        /// <param name="stockTrackings">danh sách cần thêm mới</param>
        /// <returns>bool</returns>
        Task<bool> InsertStockTrackingAsync(List<StockTracking> stockTrackings);

        /// <summary>
        /// Sửa thông tin theo dõi xử lý cổ phiếu
        /// </summary>
        /// <param name="stockTracking">Thông tin cần sửa</param>
        /// <returns>bool</returns>
        Task<bool> UpdateStockTrackingAsync(StockTracking stockTracking);
    }
}
