using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Pl.Sas.Core;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;

namespace Pl.Sas.Infrastructure.Analytics
{
    /// <summary>
    /// Lớp xử lỹ liệu cho analytic
    /// </summary>
    public class AnalyticsData : IAnalyticsData
    {
        private readonly AnalyticsDbContext _analyticsDbContext;

        public AnalyticsData(AnalyticsDbContext analyticsDbContext)
        {
            _analyticsDbContext = analyticsDbContext;
        }

        public virtual async Task<bool> SaveTestTradingResultAsync(TradingResult tradingResult)
        {
            var updateItem = _analyticsDbContext.TradingResults.FirstOrDefault(q => q.Symbol == tradingResult.Symbol && q.Principle == tradingResult.Principle);
            if (updateItem is not null)
            {
                updateItem.IsBuy = tradingResult.IsBuy;
                updateItem.BuyPrice = tradingResult.BuyPrice;
                updateItem.IsSell = tradingResult.IsSell;
                updateItem.SellPrice = tradingResult.SellPrice;
                updateItem.Capital = tradingResult.Capital;
                updateItem.Profit = tradingResult.Profit;
                updateItem.TotalTax = tradingResult.TotalTax;
                updateItem.TradingNotes = tradingResult.TradingNotes;
                updateItem.UpdatedTime = DateTime.Now;
            }
            else
            {
                _analyticsDbContext.TradingResults.Add(tradingResult);
            }
            return await _analyticsDbContext.SaveChangesAsync() > 0;
        }

        public virtual async Task<IndustryAnalytics?> GetIndustryAnalyticsAsync(string code)
        {
            return await _analyticsDbContext.IndustryAnalytics.FirstOrDefaultAsync(q => q.Code == code);
        }

        public virtual async Task<List<IndustryAnalytics>> GetIndustryAnalyticsAsync()
        {
            return await _analyticsDbContext.IndustryAnalytics.ToListAsync();
        }

        public virtual async Task<bool> SaveManualScoreAsync(string code, float manualScore)
        {
            Guard.Against.NullOrEmpty(code, nameof(code));
            var updateItem = await _analyticsDbContext.IndustryAnalytics.FirstOrDefaultAsync(q => q.Code == code);
            if (updateItem is not null)
            {
                updateItem.ManualScore = manualScore;
            }
            return await _analyticsDbContext.SaveChangesAsync() > 0;
        }

        public virtual async Task<AnalyticsResult?> GetAnalyticsResultAsync(string symbol)
        {
            return await _analyticsDbContext.AnalyticsResults.FirstOrDefaultAsync(q => q.Symbol == symbol);
        }

        public virtual async Task<List<TradingResult>> GetTradingResultAsync(string symbol)
        {
            return await _analyticsDbContext.TradingResults.Where(q => q.Symbol == symbol).ToListAsync();
        }

        public virtual async Task<bool> SaveMacroeconomicsScoreAsync(string symbol, int marketScore, byte[] marketNote)
        {
            var updateItem = _analyticsDbContext.AnalyticsResults.FirstOrDefault(q => q.Symbol == symbol);
            if (updateItem is not null)
            {
                updateItem.MarketScore = marketScore;
                updateItem.MarketNotes = marketNote;
                updateItem.UpdatedTime = DateTime.Now;
            }
            else
            {
                _analyticsDbContext.AnalyticsResults.Add(new()
                {
                    Symbol = symbol,
                    MarketScore = marketScore,
                    MarketNotes = marketNote
                });
            }
            return await _analyticsDbContext.SaveChangesAsync() > 0;
        }

        public virtual async Task<bool> SaveCompanyValueScoreAsync(string symbol, int companyValueScore, byte[] companyValueNote)
        {
            var updateItem = _analyticsDbContext.AnalyticsResults.FirstOrDefault(q => q.Symbol == symbol);
            if (updateItem is not null)
            {
                updateItem.CompanyValueScore = companyValueScore;
                updateItem.CompanyValueNotes = companyValueNote;
                updateItem.UpdatedTime = DateTime.Now;
            }
            else
            {
                _analyticsDbContext.AnalyticsResults.Add(new()
                {
                    Symbol = symbol,
                    CompanyValueScore = companyValueScore,
                    CompanyValueNotes = companyValueNote
                });
            }
            return await _analyticsDbContext.SaveChangesAsync() > 0;
        }

        public virtual async Task<bool> SaveCompanyGrowthScoreAsync(string symbol, int companyGrowthScore, byte[] companyGrowthNote)
        {
            var updateItem = _analyticsDbContext.AnalyticsResults.FirstOrDefault(q => q.Symbol == symbol);
            if (updateItem is not null)
            {
                updateItem.CompanyGrowthScore = companyGrowthScore;
                updateItem.CompanyGrowthNotes = companyGrowthNote;
                updateItem.UpdatedTime = DateTime.Now;
            }
            else
            {
                _analyticsDbContext.AnalyticsResults.Add(new()
                {
                    Symbol = symbol,
                    CompanyGrowthScore = companyGrowthScore,
                    CompanyGrowthNotes = companyGrowthNote
                });
            }
            return await _analyticsDbContext.SaveChangesAsync() > 0;
        }

        public virtual async Task<bool> SaveStockScoreAsync(string symbol, int stockScore, byte[] stockNote)
        {
            var updateItem = _analyticsDbContext.AnalyticsResults.FirstOrDefault(q => q.Symbol == symbol);
            if (updateItem is not null)
            {
                updateItem.StockScore = stockScore;
                updateItem.StockNotes = stockNote;
                updateItem.UpdatedTime = DateTime.Now;
            }
            else
            {
                _analyticsDbContext.AnalyticsResults.Add(new()
                {
                    Symbol = symbol,
                    StockScore = stockScore,
                    StockNotes = stockNote
                });
            }
            return await _analyticsDbContext.SaveChangesAsync() > 0;
        }

        public virtual async Task<bool> SaveFiinScoreAsync(string symbol, int fiinScore, byte[] fiinNote)
        {
            var updateItem = _analyticsDbContext.AnalyticsResults.FirstOrDefault(q => q.Symbol == symbol);
            if (updateItem is not null)
            {
                updateItem.FiinScore = fiinScore;
                updateItem.FiinNotes = fiinNote;
                updateItem.UpdatedTime = DateTime.Now;
            }
            else
            {
                _analyticsDbContext.AnalyticsResults.Add(new()
                {
                    Symbol = symbol,
                    FiinScore = fiinScore,
                    FiinNotes = fiinNote
                });
            }
            return await _analyticsDbContext.SaveChangesAsync() > 0;
        }

        public virtual async Task<bool> SaveVndScoreAsync(string symbol, int vndScore, byte[] vndNote)
        {
            var updateItem = _analyticsDbContext.AnalyticsResults.FirstOrDefault(q => q.Symbol == symbol);
            if (updateItem is not null)
            {
                updateItem.VndScore = vndScore;
                updateItem.VndNote = vndNote;
                updateItem.UpdatedTime = DateTime.Now;
            }
            else
            {
                _analyticsDbContext.AnalyticsResults.Add(new()
                {
                    Symbol = symbol,
                    VndScore = vndScore,
                    VndNote = vndNote
                });
            }
            return await _analyticsDbContext.SaveChangesAsync() > 0;
        }

        public virtual async Task<bool> SaveTargetPriceAsync(string symbol, float targetPrice, byte[] targetPriceNote)
        {
            var updateItem = _analyticsDbContext.AnalyticsResults.FirstOrDefault(q => q.Symbol == symbol);
            if (updateItem is not null)
            {
                updateItem.TargetPrice = targetPrice;
                updateItem.TargetPriceNotes = targetPriceNote;
                updateItem.UpdatedTime = DateTime.Now;
            }
            else
            {
                _analyticsDbContext.AnalyticsResults.Add(new()
                {
                    Symbol = symbol,
                    TargetPrice = targetPrice,
                    TargetPriceNotes = targetPriceNote
                });
            }
            return await _analyticsDbContext.SaveChangesAsync() > 0;
        }

        public virtual async Task<bool> SaveIndustryScoreAsync(string code, float score, byte[] analyticsNote)
        {
            var updateItem = _analyticsDbContext.IndustryAnalytics.FirstOrDefault(q => q.Code == code);
            if (updateItem is not null)
            {
                updateItem.Score = score;
                updateItem.Notes = analyticsNote;
                updateItem.UpdatedTime = DateTime.Now;
            }
            else
            {
                _analyticsDbContext.IndustryAnalytics.Add(new()
                {
                    Code = code,
                    Score = score,
                    Notes = analyticsNote,
                });
            }
            return await _analyticsDbContext.SaveChangesAsync() > 0;
        }
    }
}
