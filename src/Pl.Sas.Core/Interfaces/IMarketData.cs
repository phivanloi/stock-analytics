using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    /// <summary>
    /// Lớp xử lỹ liệu của market
    /// </summary>
    public interface IMarketData
    {
        /// <summary>
        /// Thêm mới một danh sách lịch
        /// </summary>
        /// <param name="schedules">Danh sách lịch cần thêm mới</param>
        /// <returns>bool</returns>
        Task<bool> InsertScheduleAsync(List<Schedule> schedules);

        /// <summary>
        /// Hàm tạo và update các cổ phiếu
        /// </summary>
        /// <param name="insertItems">Danh sách các cổ phiếu cần tạo</param>
        /// <param name="updateItems">Danh sách các cổ phiếu cần update</param>
        /// <returns>bool</returns>
        Task<bool> InitialStockAsync(List<Stock> insertItems, List<Stock> updateItems);

        /// <summary>
        /// Lấy thông tin đầy đủ của một Schedule
        /// </summary>
        /// <param name="id">Id cần lấy</param>
        /// <returns>Schedule</returns>
        Task<Schedule?> GetScheduleByIdAsync(string id);

        /// <summary>
        /// Lấy thông tin đầy đủ của một Schedule trên cache
        /// </summary>
        /// <param name="id">Id cần lấy</param>
        /// <returns>Schedule</returns>
        Task<Schedule?> CacheGetScheduleByIdAsync(string id);

        /// <summary>
        /// Lấy danh sách toàn bộ mã trong hệ thống đang có và chuyển sang dictionary symbol, obj
        /// </summary>
        /// <returns>Dictionary<string, Stock></returns>
        Task<Dictionary<string, Stock>> GetStockDictionaryAsync();
    }
}
