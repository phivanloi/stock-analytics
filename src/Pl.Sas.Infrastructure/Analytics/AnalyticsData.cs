using Microsoft.EntityFrameworkCore;
using Pl.Sas.Core;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Infrastructure.Analytics;

namespace Pl.Sas.Infrastructure.Data
{
    /// <summary>
    /// Lớp xử lỹ liệu của market
    /// </summary>
    public class AnalyticsData : IAnalyticsData
    {
        private readonly AnalyticsDbContext _analyticsDbContext;
        private readonly IMemoryCacheService _memoryCacheService;

        public AnalyticsData(
            IMemoryCacheService memoryCacheService,
            AnalyticsDbContext analyticsDbContext)
        {
            _analyticsDbContext = analyticsDbContext;
            _memoryCacheService = memoryCacheService;
        }

        public virtual async Task<bool> UpdateStockTrackingAsync(StockTracking stockTracking)
        {
            var updateItem = await _analyticsDbContext.StockTrackings.FirstOrDefaultAsync(s => s.Symbol == stockTracking.Symbol);
            if (updateItem is not null)
            {
                updateItem.DownloadStatus = stockTracking.DataStatus;
                updateItem.DownloadDate = stockTracking.DownloadDate;
                updateItem.DataStatus = stockTracking.DataStatus;
                updateItem.DataDate = stockTracking.DataDate;
                updateItem.AnalyticsStatus = stockTracking.AnalyticsStatus;
                updateItem.AnalyticsDate = stockTracking.AnalyticsDate;
                return await _analyticsDbContext.SaveChangesAsync() > 0;
            }
            return false;
        }

        public virtual async Task<bool> InsertStockTrackingAsync(List<StockTracking> stockTrackings)
        {
            if (stockTrackings.Count > 0)
            {
                _analyticsDbContext.StockTrackings.AddRange(stockTrackings);
                return await _analyticsDbContext.SaveChangesAsync() > 0;
            }
            return false;
        }
    }
}
