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
    }
}
