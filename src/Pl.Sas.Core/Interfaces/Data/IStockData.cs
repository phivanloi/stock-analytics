using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    public interface IStockData
    {
        /// <summary>
        /// Thêm mới một loạt thông tin cổ phiểu
        /// </summary>
        /// <param name="stocks">Danh sách cần thêm mới</param>
        /// <returns></returns>
        Task BulkInserAsync(List<Stock> stocks);

        /// <summary>
        /// Sử một loạt cổ phiếu
        /// </summary>
        /// <param name="stocks">Danh sách cổ phiếu cần sửa</param>
        /// <returns></returns>
        Task BulkUpdateAsync(List<Stock> stocks);

        /// <summary>
        /// Lấy ra toàn bộ mã chứng khoán trong hệ thống
        /// </summary>
        /// <param name="type">Loại cổ phiếu</param>
        /// <returns>IEnumerable Stock</returns>
        Task<IEnumerable<Stock>> FindAllAsync(string? type = "s");

        /// <summary>
        /// Lấy toán bộ thông tin cổ phiếu theo mã cổ phiếu
        /// </summary>
        /// <param name="code">Mã cổ phiếu</param>
        /// <returns>Stock</returns>
        Task<Stock> GetByCodeAsync(string code);

        /// <summary>
        /// xóa một cổ phiếu theo id
        /// </summary>
        /// <param name="id">Id cần xóa</param>
        /// <returns>bool</returns>
        Task<bool> DeleteAsync(string id);

        /// <summary>
        /// Lấy danh sách sàn
        /// </summary>
        /// <returns>string</returns>
        Task<List<string>> GetExchanges();
    }
}