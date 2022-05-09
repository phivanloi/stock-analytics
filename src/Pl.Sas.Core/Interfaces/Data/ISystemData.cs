using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    /// <summary>
    /// Lớp xử lỹ liệu của các thành phần trong hệ thống
    /// </summary>
    public interface ISystemData
    {
        /// <summary>
        /// Lấy lịch theo loại vào khóa dữ liệu
        /// </summary>
        /// <param name="type">Loại</param>
        /// <param name="dataKey">Khóa dữ liệu</param>
        /// <returns>Schedule</returns>
        Task<Schedule?> GetScheduleAsync(int type, string dataKey);

        /// <summary>
        /// Lấy thông tin khóa giá trị
        /// </summary>
        /// <param name="key">Khóa</param>
        /// <returns>KeyValue?</returns>
        Task<KeyValue?> GetKeyValueAsync(string key);

        /// <summary>
        /// Ghi lại khóa giá trị
        /// </summary>
        /// <typeparam name="T">Loại giá trị</typeparam>
        /// <param name="key">Khóa</param>
        /// <param name="value">Giá trị</param>
        /// <returns>bool</returns>
        Task<bool> SetKeyValueAsync<T>(string key, T value);

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
        /// Thêm mới một danh sách lịch
        /// </summary>
        /// <param name="schedules">Danh sách lịch cần thêm mới</param>
        /// <returns>bool</returns>
        Task<bool> InsertScheduleAsync(List<Schedule> schedules);

        /// <summary>
        /// Hàm update lịch
        /// </summary>
        /// <param name="schedule">Thông tin lịch cần update</param>
        /// <returns>bool</returns>
        Task<bool> UpdateScheduleAsync(Schedule schedule);
    }
}
