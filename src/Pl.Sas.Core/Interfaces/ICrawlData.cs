using Pl.Sas.Core.Entities;

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
    }
}
