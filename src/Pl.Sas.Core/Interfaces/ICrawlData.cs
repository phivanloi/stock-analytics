using Pl.Sas.Core.Entities.CrawlObjects;

namespace Pl.Sas.Core.Interfaces
{
    public interface ICrawlData
    {
        /// <summary>
        /// Lấy danh sách cổ phiếu bắt đầu từ ssi
        /// </summary>
        /// <returns>SsiAllStock?</returns>
        Task<SsiAllStock?> DownloadInitialMarketStockAsync();

        /// <summary>
        /// Lấy thông tin công ty
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <returns>SsiCompanyInfo</returns>
        Task<SsiCompanyInfo?> DownloadCompanyInfoAsync(string symbol);

        /// <summary>
        /// Lấy thông tin lãnh đạo của công ty
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <returns>SsiLeadership?</returns>
        Task<SsiLeadership?> DownloadLeadershipAsync(string symbol);

        /// <summary>
        /// Lấy thông tin vốn và cổ tức
        /// </summary>
        /// <param name="symbol">Mã chứng khoán</param>
        /// <returns>SsiCapitalAndDividend?</returns>
        Task<SsiCapitalAndDividend?> DownloadCapitalAndDividendAsync(string symbol);

        /// <summary>
        /// Tải dữ liệu lịch sử giá theo ngày
        /// </summary>
        /// <param name="symbol">mã chứng khoán</param>
        /// <param name="size">Số lịch sử giá cần lấy</param>
        /// <returns>SsiFinancialIndicator</returns>
        Task<SsiStockPriceHistory?> DownloadStockPricesAsync(string symbol, int size = 10000);

        /// <summary>
        /// Tải dữ liệu đánh giá cổ phiếu của fiin
        /// </summary>
        /// <param name="symbol">mã chứng khoán</param>
        /// <returns>FiintradingEvaluate</returns>
        Task<FiintradingEvaluate?> DownloadFiinStockEvaluateAsync(string symbol);

        /// <summary>
        /// Tải sự kiện của công ty
        /// </summary>
        /// <param name="symbol">mã chứng khoán</param>
        /// <param name="size">Số bản ghi cần lấy</param>
        /// <returns>SsiCorporateAction</returns>
        Task<SsiCorporateAction?> DownloadCorporateActionAsync(string symbol, int size = 10000);

        /// <summary>
        /// Lấy danh sách báo cáo khuyến nghị
        /// </summary>
        /// <param name="symbol">mã chứng khoán</param>
        /// <returns>VndRecommendations</returns>
        Task<VndRecommendations?> DownloadStockRecommendationsAsync(string symbol);

        /// <summary>
        /// Tải đánh giá cố phiểu của vnd
        /// </summary>
        /// <param name="symbol">mã chứng khoán</param>
        /// <returns>VndStockScorings?</returns>
        Task<VndStockScorings?> DownloadVndStockScoringsAsync(string symbol);

        /// <summary>
        /// Lấy dữ liệu tài chính của công ty
        /// </summary>
        /// <param name="symbol">mã chứng khoán</param>
        /// <returns>SsiFinancialIndicator?</returns>
        Task<SsiFinancialIndicator?> DownloadFinancialIndicatorAsync(string symbol);
    }
}
