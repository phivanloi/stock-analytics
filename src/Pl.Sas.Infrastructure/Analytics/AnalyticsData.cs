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

        public virtual async Task<bool> SaveMacroeconomicsScoreAsync(string symbol, DateTime tradingDate, int marketScore, byte[] marketNote)
        {
            var updateItem = _analyticsDbContext.AnalyticsResults.FirstOrDefault(q => q.Symbol == symbol && q.TradingDate == tradingDate);
            if (updateItem is not null)
            {
                updateItem.MarketScore = marketScore;
                updateItem.MarketNotes = marketNote;
            }
            else
            {
                _analyticsDbContext.AnalyticsResults.Add(new()
                {
                    Symbol = symbol,
                    TradingDate = tradingDate,
                    MarketScore = marketScore,
                    MarketNotes = marketNote
                });
            }
            return await _analyticsDbContext.SaveChangesAsync() > 0;
        }

        public virtual async Task<bool> SaveCompanyValueScoreAsync(string symbol, DateTime tradingDate, int companyValueScore, byte[] companyValueNote)
        {
            var updateItem = _analyticsDbContext.AnalyticsResults.FirstOrDefault(q => q.Symbol == symbol && q.TradingDate == tradingDate);
            if (updateItem is not null)
            {
                updateItem.CompanyValueScore = companyValueScore;
                updateItem.CompanyValueNotes = companyValueNote;
            }
            else
            {
                _analyticsDbContext.AnalyticsResults.Add(new()
                {
                    Symbol = symbol,
                    TradingDate = tradingDate,
                    CompanyValueScore = companyValueScore,
                    CompanyValueNotes = companyValueNote
                });
            }
            return await _analyticsDbContext.SaveChangesAsync() > 0;
        }

        public virtual async Task<bool> SaveCompanyGrowthScoreAsync(string symbol, DateTime tradingDate, int companyGrowthScore, byte[] companyGrowthNote)
        {
            var updateItem = _analyticsDbContext.AnalyticsResults.FirstOrDefault(q => q.Symbol == symbol && q.TradingDate == tradingDate);
            if (updateItem is not null)
            {
                updateItem.CompanyGrowthScore = companyGrowthScore;
                updateItem.CompanyGrowthNotes = companyGrowthNote;
            }
            else
            {
                _analyticsDbContext.AnalyticsResults.Add(new()
                {
                    Symbol = symbol,
                    TradingDate = tradingDate,
                    CompanyGrowthScore = companyGrowthScore,
                    CompanyGrowthNotes = companyGrowthNote
                });
            }
            return await _analyticsDbContext.SaveChangesAsync() > 0;
        }

        public virtual async Task<bool> SaveStockScoreAsync(string symbol, DateTime tradingDate, int stockScore, byte[] stockNote)
        {
            var updateItem = _analyticsDbContext.AnalyticsResults.FirstOrDefault(q => q.Symbol == symbol && q.TradingDate == tradingDate);
            if (updateItem is not null)
            {
                updateItem.StockScore = stockScore;
                updateItem.StockNotes = stockNote;
            }
            else
            {
                _analyticsDbContext.AnalyticsResults.Add(new()
                {
                    Symbol = symbol,
                    TradingDate = tradingDate,
                    StockScore = stockScore,
                    StockNotes = stockNote
                });
            }
            return await _analyticsDbContext.SaveChangesAsync() > 0;
        }

        public virtual async Task<bool> SaveFiinScoreAsync(string symbol, DateTime tradingDate, int fiinScore, byte[] fiinNote)
        {
            var updateItem = _analyticsDbContext.AnalyticsResults.FirstOrDefault(q => q.Symbol == symbol && q.TradingDate == tradingDate);
            if (updateItem is not null)
            {
                updateItem.FiinScore = fiinScore;
                updateItem.FiinNotes = fiinNote;
            }
            else
            {
                _analyticsDbContext.AnalyticsResults.Add(new()
                {
                    Symbol = symbol,
                    TradingDate = tradingDate,
                    FiinScore = fiinScore,
                    FiinNotes = fiinNote
                });
            }
            return await _analyticsDbContext.SaveChangesAsync() > 0;
        }

        public virtual async Task<bool> SaveVndScoreAsync(string symbol, DateTime tradingDate, int vndScore, byte[] vndNote)
        {
            var updateItem = _analyticsDbContext.AnalyticsResults.FirstOrDefault(q => q.Symbol == symbol && q.TradingDate == tradingDate);
            if (updateItem is not null)
            {
                updateItem.VndScore = vndScore;
                updateItem.VndNote = vndNote;
            }
            else
            {
                _analyticsDbContext.AnalyticsResults.Add(new()
                {
                    Symbol = symbol,
                    TradingDate = tradingDate,
                    VndScore = vndScore,
                    VndNote = vndNote
                });
            }
            return await _analyticsDbContext.SaveChangesAsync() > 0;
        }

        public virtual async Task<bool> SaveTargetPriceAsync(string symbol, DateTime tradingDate, float targetPrice, byte[] targetPriceNote)
        {
            var updateItem = _analyticsDbContext.AnalyticsResults.FirstOrDefault(q => q.Symbol == symbol && q.TradingDate == tradingDate);
            if (updateItem is not null)
            {
                updateItem.TargetPrice = targetPrice;
                updateItem.TargetPriceNotes = targetPriceNote;
            }
            else
            {
                _analyticsDbContext.AnalyticsResults.Add(new()
                {
                    Symbol = symbol,
                    TradingDate = tradingDate,
                    TargetPrice = targetPrice,
                    TargetPriceNotes = targetPriceNote
                });
            }
            return await _analyticsDbContext.SaveChangesAsync() > 0;
        }
    }
}
