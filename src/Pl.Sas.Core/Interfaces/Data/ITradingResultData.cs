using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    public interface ITradingResultData
    {
        /// <summary>
        /// Lấy toàn bộ kết quả trading trong hệ thống
        /// </summary>
        /// <returns>List TradingResult</returns>
        Task<List<TradingResult>> FindAllAsync();

        /// <summary>
        /// Lấy danh sách kết quả đầu tư thử nghiệm
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <param name="principle">Id phương pháp</param>
        /// <returns>List TradingResult</returns>
        Task<List<TradingResult>> FindAllAsync(string symbol, int? principle = null);

        /// <summary>
        /// Ghi một kết quả trading
        /// </summary>
        /// <param name="tradingResult">Kết quả cần ghi</param>
        /// <returns>bool</returns>
        Task<bool> SaveTestTradingResultAsync(TradingResult tradingResult);

        /// <summary>
        /// Lấy danh sách tối ưu cho hiển thị
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <returns></returns>
        Task<List<TradingResult>?> GetForViewAsync(string symbol);
    }
}