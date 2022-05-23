using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    public interface IScheduleData
    {
        /// <summary>
        /// Lấy một danh sách Schedule phù hợp để kích hoạt sự kiện
        /// </summary>
        /// <param name="selectTime">Thời gian muốn lấy</param>
        /// <param name="top">Số bản ghi cần lấy</param>
        /// <returns>IReadOnlyList Schedule</returns>
        Task<IReadOnlyList<Schedule>> GetForActiveEventAsync(DateTime selectTime, int top = 10);

        /// <summary>
        /// Update thời gian kích hoạt sự kiện của một Schedule
        /// </summary>
        /// <param name="id">Id Schedule cần update</param>
        /// <param name="setTime">Thời gian</param>
        /// <returns>bool</returns>
        Task<bool> SetActiveTimeAsync(string id, DateTime setTime);

        /// <summary>
        /// Update một tập hợp Schedule
        /// </summary>
        /// <param name="schedules">Tập hợp cần update, chỉ cần id và activetime</param>
        /// <returns>Task</returns>
        Task BulkSetActiveTimeAsync(IEnumerable<Schedule> schedules);

        /// <summary>
        /// Thêm mới một danh sách Schedule
        /// </summary>
        /// <param name="sitemaps"></param>
        /// <returns>Task</returns>
        Task BulkInserAsync(IEnumerable<Schedule> sitemaps);

        /// <summary>
        /// Lấy thông tin đầy đủ của một Schedule
        /// </summary>
        /// <param name="id">Id cần lấy</param>
        /// <returns>Schedule</returns>
        Task<Schedule> GetByIdAsync(string id);

        /// <summary>
        /// Xóa một Schedule
        /// </summary>
        /// <param name="id">id cần xóa</param>
        /// <returns>bool</returns>
        Task<bool> DeleteAsync(string id);

        /// <summary>
        /// Sửa thông tin lịch
        /// </summary>
        /// <param name="schedule">Thông tin lịch</param>
        /// <returns></returns>
        Task<bool> UpdateAsync(Schedule schedule);

        /// <summary>
        /// Thêm mới một lịch
        /// </summary>
        /// <param name="schedule">Thông tin lịch</param>
        /// <returns>bool</returns>
        Task<bool> InsertAsync(Schedule schedule);

        /// <summary>
        /// Hàm update giúp kích hoạt một loại lịch nào đó ngay lập tức
        /// </summary>
        /// <param name="type">Loại lịch</param>
        /// <param name="code">Mã chứng khoán khi muốn kích hoạt theo một mã nhất định</param>
        /// <returns>bool</returns>
        Task<bool> UtilityUpdateAsync(int type, string code);

        /// <summary>
        /// Lấy một lịch theo loại và theo data key
        /// </summary>
        /// <param name="type">Loại lịch</param>
        /// <param name="dataKey">data key</param>
        /// <returns>Schedule Schedule</returns>
        Task<Schedule?> FindAsync(int type, string dataKey);
    }
}