using Pl.Sas.Core.Entities.DownloadObjects;

namespace Pl.Sas.Core.Interfaces
{
    //Tổng hợp các nghiệp vụ download dữ liệu
    public interface IDownloadData
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
        Task<FiintradingEvaluate?> DownloadFiinStockEvaluatesAsync(string symbol);

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

        /// <summary>
        /// Lấy dữ liệu giao dịch trong 1 phiên
        /// </summary>
        /// <param name="stockNo">Mã chứng khoán bên ssi</param>
        /// <returns>SsiTransaction?</returns>
        Task<SsiTransaction?> DownloadTransactionAsync(string stockNo);

        /// <summary>
        /// Tải thông tin chỉ số
        /// </summary>
        /// <param name="symbol">Mã chỉ số</param>
        /// <param name="configTime">Cấu hình thời gian bắt đầu lấy</param>
        /// <param name="type">Loại chart</param>
        /// <returns>List SsiIndexPrice</returns>
        Task<List<SsiChartPrice>> DownloadSsiChartPricesAsync(string symbol, long configTime, string type = "D");

        /// <summary>
        /// Lấy lãi suất ngân hàng cao nhất với các kỳ hạn được chuyền vào
        /// </summary>
        /// <param name="periods">Danh sách kỳ hạn cách nhau 1 dấu ,</param>
        /// <returns>List BankInterestRateObject</returns>
        Task<List<BankInterestRateObject>> DownloadBankInterestRateAsync(string periods = "3,6,12,24");
    }
}
