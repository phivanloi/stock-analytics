using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    public interface IStockPriceData
    {
        /// <summary>
        /// Lấy bản ghi trading mới nhất.
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <returns></returns>
        Task<StockPrice> GetLastAsync(string symbol);

        /// <summary>
        /// Thêm mới một danh sách lịch sử giá chứng khoán
        /// </summary>
        /// <param name="stockPrices">Danh sách lịch sử giá</param>
        /// <returns>Task</returns>
        Task BulkInserAsync(List<StockPrice> stockPrices);

        /// <summary>
        /// Lấy một lịch sử giá chứng khoán dựa vào mã chứng khoán và ngày giao dịch
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <param name="tradingDate">Ngày giao dịch</param>
        /// <returns>Stock</returns>
        Task<StockPrice> GetByDayAsync(string symbol, DateTime tradingDate);

        /// <summary>
        /// Update một mã chứng khoán
        /// </summary>
        /// <param name="stockPrice">đói tượng chứng khoán cần update</param>
        /// <returns>bool</returns>
        Task<bool> UpdateAsync(StockPrice stockPrice);

        /// <summary>
        /// Xóa một lịch sử giá chứng khoán
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> DeleteAsync(string id);

        /// <summary>
        /// Hàm lấy lịch sử giá cho hiển thị ở trang chi tiết
        /// </summary>
        /// <param name="symbol">mã cổ phiếu</param>
        /// <param name="numberItem">Số bản ghi cần lấy</param>
        /// <returns>List StockPrice</returns>
        Task<List<StockPrice>> GetForDetailPageAsync(string symbol, int numberItem = 10000);

        /// <summary>
        /// Lấy toàn bộ theo mã cố phiếu
        /// </summary>
        /// <param name="symbol">mã cổ phiếu</param>
        /// <param name="numberItem">Số bản ghi cần lấy</param>
        /// <param name="fromTradingTime">bắt đầu từ ngày giao dịch</param>
        /// <param name="toTradingTime">kết thúc đến ngày giao dịch</param>
        /// <returns>IEnumerable StockPrice</returns>
        Task<List<StockPrice>> FindAllAsync(string symbol, int numberItem = 10000, DateTime? fromTradingTime = null, DateTime? toTradingTime = null);

        /// <summary>
        /// Lấy danh sách lịch sử giá dùng để trading
        /// </summary>
        /// <param name="symbol">mã cổ phiếu</param>
        /// <param name="numberItem">Số bản ghi cần lấy</param>
        /// <param name="fromTradingTime">bắt đầu từ ngày giao dịch</param>
        /// <param name="toTradingTime">kết thúc đến ngày giao dịch</param>
        /// <returns>List StockPrice</returns>
        Task<List<StockPrice>> FindAllForTradingAsync(string symbol, int numberItem = 10000, DateTime? fromTradingTime = null, DateTime? toTradingTime = null);

        /// <summary>
        /// Thêm mới một lịch sử giá cổ phiếu
        /// </summary>
        /// <param name="stockPrice">stockPrice</param>
        /// <returns></returns>
        Task<bool> InsertAsync(StockPrice stockPrice);

        /// <summary>
        /// Lấy danh sách lịch sử giá cổ phiếu sử dụng cho các hàm tọa dữ liệu cho máy học
        /// </summary>
        /// <param name="top">Số bản ghi cần lấy</param>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <returns></returns>
        Task<List<StockPrice>> GetTopForBuildMlTrainingDataAsync(int top, string symbol);

        /// <summary>
        /// Lấy danh sách lịch sử giá cho phần phân tích ngành
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <param name="top">Số bản ghi cần lấy</param>
        /// <returns>List StockPrice</returns>
        Task<List<StockPrice>> GetForIndustryTrendAnalyticsAsync(string symbol, int top);

        /// <summary>
        /// hàm lấy lịch sử giá cho việc hiển thị
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <param name="numberItem">Số bản ghi cần lấy</param>
        /// <returns></returns>
        Task<List<StockPrice>> GetForStockViewAsync(string symbol, int numberItem = 10000);

        /// <summary>
        /// Lấy danh sách dữ liệu cho máy học phân loại trạng thái tăng giá
        /// </summary>
        /// <param name="top">số lượng bạn ghi cần lấy</param>
        /// <param name="symbol">Mã chứng khoán càn lấy</param>
        /// <returns>List StockPrice</returns>
        Task<List<StockPrice>> GetTopForBuildMlClassificationTrainingDataAsync(int top, string symbol);

        /// <summary>
        /// Xóa toàn bộ lịch sử giá của một mã chứng khoán
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu cần xóa</param>
        /// <returns></returns>
        Task<bool> DeleteBySymbolAsync(string symbol);

        /// <summary>
        /// Lấy danh sách cho việc phát hiện trend của thị trường
        /// </summary>
        /// <param name="symbol">Mã chứng khoán càn lấy</param>
        /// <param name="top">số lượng bạn ghi cần lấy</param>
        /// <returns></returns>
        Task<List<StockPrice>> GetForIMarketSentimentAnalyticsAsync(string symbol, int top);

        /// <summary>
        /// Kiểm tra xem lịch sử giá đã có item này hay chưa
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <param name="tradingDate">Ngày giao dịch</param>
        /// <returns></returns>
        Task<bool> IsExistByDayAsync(string symbol, DateTime tradingDate);
    }
}