using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    public interface IChartPriceData
    {
        /// <summary>
        /// Lấy toàn bộ lịch sử giá chart theo mã chứng khoán
        /// </summary>
        /// <param name="symbol">Mã chứng khoán cần lấy</param>
        /// <param name="type">Loại</param>
        /// <returns></returns>
        Task<List<ChartPrice>?> CacheFindAllAsync(string symbol, string type = "D");

        /// <summary>
        /// Lấy danh sách
        /// </summary>
        /// <param name="symbol">Mã chứng khoán cần lấy</param>
        /// <param name="type">Loại</param>
        /// <param name="fromDate">Từ ngày</param>
        /// <param name="toDate">Đến ngày</param>
        /// <returns></returns>
        Task<List<ChartPrice>> FindAllAsync(string symbol, string type = "D", DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Cập nhập lại toàn bộ dữ liệu chart
        /// </summary>
        /// <param name="chartPrices">Danh sách dữ liệu mới</param>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <param name="type">Loại chart</param>
        /// <returns>bool</returns>
        Task<bool> ResetChartPriceAsync(List<ChartPrice> chartPrices, string symbol, string type = "D");

        /// <summary>
        /// Thêm mới danh sách
        /// </summary>
        /// <param name="chartPrices">Danh sách cần thêm mới</param>
        /// <returns></returns>
        Task BulkInserAsync(IEnumerable<ChartPrice> chartPrices);

        /// <summary>
        /// Thực hiện kiểm tra nếu chưa có thì thêm mới có rồi thì update giá trị
        /// </summary>
        /// <param name="chartPrice">Dữ liệ cần check</param>
        /// <returns>bool</returns>
        Task<bool> UpsertAsync(ChartPrice chartPrice);

        /// <summary>
        /// Xóa danh sách theo điều kiện
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <param name="type">Loại</param>
        /// <returns>bool</returns>
        Task<bool> DeleteAsync(string symbol, string type);
    }
}