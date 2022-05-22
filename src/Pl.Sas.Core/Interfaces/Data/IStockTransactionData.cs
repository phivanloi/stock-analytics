using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    public interface IStockTransactionData
    {
        /// <summary>
        /// Lấy danh sách chi tiết khớp lệnh của cổ phiếu
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <param name="tradingDate">ngày giao dịch</param>
        /// <returns>List StockTransaction</returns>
        Task<List<StockTransaction>> FindAllAsync(string symbol, DateTime? tradingDate = null);

        /// <summary>
        /// Lấy chi tiết một phiên khớp lệnh của cổ phiếu
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <param name="tradingDate">ngày giao dịch</param>
        /// <returns>StockTransaction</returns>
        Task<StockTransaction> FindAsync(string symbol, DateTime? tradingDate = null);

        /// <summary>
        /// Ghi lại chi tiết khớp lệnh của một phiên giao dịch
        /// </summary>
        /// <param name="stockTransaction">Thông tin chi tiết</param>
        /// <returns>bool</returns>
        Task<bool> SaveStockTransactionAsync(StockTransaction stockTransaction);

        /// <summary>
        /// Lấy danh sách khớp lệnh cho việc trading
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <param name="startDate">Ngày bắt đầu</param>
        /// <param name="endDate">Ngày kết thúc</param>
        /// <returns>List StockTransaction</returns>
        Task<List<StockTransaction>> GetForTradingAsync(string symbol, DateTime? startDate = null, DateTime? endDate = null);
    }
}