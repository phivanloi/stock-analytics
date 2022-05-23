using Pl.Sas.Core.Entities;

namespace Pl.Sas.Core.Interfaces
{
    public interface IStockRecommendationData
    {
        /// <summary>
        /// Lấy danh sách báo cáo đề xuất
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <param name="startDate">Bắt đầu tư ngày</param>
        /// <returns>List StockRecommendation</returns>
        Task<List<StockRecommendation>> FindAllAsync(string symbol, DateTime? startDate);

        /// <summary>
        /// Ghi lại báo cáo đề xuất của các công ty chứng khoán
        /// </summary>
        /// <param name="stockRecommendation">Thông tin chi tiết</param>
        /// <returns>bool</returns>
        Task<bool> SaveStockRecommendationAsync(StockRecommendation stockRecommendation);

        /// <summary>
        /// Lấy top báo cáo gần nhất trong vòng 6 tháng
        /// </summary>
        /// <param name="symbol">Mã cổ phiếu</param>
        /// <param name="top">số báo cáo cần lấy</param>
        /// <returns>List StockRecommendation</returns>
        Task<List<StockRecommendation>> GetTopReportInSixMonthAsync(string symbol, int top = 5);
    }
}