using Microsoft.EntityFrameworkCore;
using Pl.Sas.Core.Entities;

namespace Pl.Sas.Scheduler.DataContexts
{
    public class MarketDbContext : DbContext
    {
        public MarketDbContext(DbContextOptions<MarketDbContext> options) : base(options) { }

        public virtual DbSet<Stock> Stocks { get; set; } = null!;
        public virtual DbSet<Company> Companies { get; set; } = null!;
        public virtual DbSet<Industry> Industries { get; set; } = null!;
        public virtual DbSet<StockPrice> StockPrices { get; set; } = null!;
        public virtual DbSet<Leadership> Leaderships { get; set; } = null!;
        public virtual DbSet<FinancialGrowth> FinancialGrowths { get; set; } = null!;
        public virtual DbSet<FinancialIndicator> FinancialIndicators { get; set; } = null!;
        public virtual DbSet<CorporateAction> CorporateActions { get; set; } = null!;
        public virtual DbSet<FiinEvaluated> FiinEvaluates { get; set; } = null!;
        public virtual DbSet<StockRecommendation> StockRecommendations { get; set; } = null!;
        public virtual DbSet<VndStockScore> VndStockScores { get; set; } = null!;
        public virtual DbSet<StockTransaction> StockTransactions { get; set; } = null!;
        public virtual DbSet<ChartPrice> ChartPrices { get; set; } = null!;
        public virtual DbSet<Schedule> Schedules { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Stock

            modelBuilder.Entity<Stock>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(22).IsRequired();
                b.Property(p => p.Symbol).HasMaxLength(16).IsRequired();
                b.Property(p => p.Name).HasMaxLength(128);
                b.Property(p => p.FullName).HasMaxLength(256);
                b.Property(p => p.Exchange).HasMaxLength(16);
                b.Property(p => p.Type).HasMaxLength(16);
                b.HasIndex(p => new { p.Symbol, p.Type });
            });

            #endregion
            #region Company

            modelBuilder.Entity<Company>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(22).IsRequired();
                b.Property(p => p.Symbol).HasMaxLength(16).IsRequired();
                b.Property(p => p.SubsectorCode).HasMaxLength(22);
                b.Property(p => p.IndustryName).HasMaxLength(128);
                b.Property(p => p.Supersector).HasMaxLength(128);
                b.Property(p => p.Sector).HasMaxLength(128);
                b.Property(p => p.Subsector).HasMaxLength(128);
                b.Property(p => p.CompanyName).HasMaxLength(256);
                b.HasIndex(p => new { p.Symbol, p.SubsectorCode });
            });

            #endregion
            #region Industry

            modelBuilder.Entity<Industry>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(22).IsRequired();
                b.Property(p => p.Code).HasMaxLength(16).IsRequired();
                b.Property(p => p.Name).HasMaxLength(256);
                b.HasIndex(i => i.Code);
            });

            #endregion
            #region Schedule

            modelBuilder.Entity<Schedule>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(22).IsRequired();
                b.Property(p => p.Name).HasMaxLength(128);
                b.Property(p => p.DataKey).HasMaxLength(64);
                b.HasIndex(p => p.ActiveTime);
            });

            #endregion
            #region StockPrice

            modelBuilder.Entity<StockPrice>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(22).IsRequired();
                b.Property(p => p.Symbol).HasMaxLength(16).IsRequired();
                b.HasIndex(p => p.Symbol);
                b.HasIndex(p => p.TradingDate);
                b.HasIndex(p => new { p.Symbol, p.TradingDate });
            });

            #endregion
            #region ChartPrice

            modelBuilder.Entity<ChartPrice>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(22).IsRequired();
                b.Property(p => p.Symbol).HasMaxLength(16).IsRequired();
                b.Property(p => p.Type).HasMaxLength(8).IsRequired();
                b.HasIndex(p => p.Symbol);
                b.HasIndex(p => p.TradingDate);
                b.HasIndex(p => new { p.Symbol, p.Type, p.TradingDate });
            });

            #endregion
            #region Leadership

            modelBuilder.Entity<Leadership>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(22).IsRequired();
                b.Property(p => p.Symbol).HasMaxLength(16).IsRequired();
                b.Property(p => p.FullName).HasMaxLength(128);
                b.Property(p => p.PositionName).HasMaxLength(128);
                b.Property(p => p.PositionLevel).HasMaxLength(128);
                b.HasIndex(p => p.Symbol);
            });

            #endregion
            #region FinancialGrowth

            modelBuilder.Entity<FinancialGrowth>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(22).IsRequired();
                b.Property(p => p.Symbol).HasMaxLength(16).IsRequired();
                b.HasIndex(p => p.Symbol);
            });

            #endregion
            #region FinancialIndicator

            modelBuilder.Entity<FinancialIndicator>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(22).IsRequired();
                b.Property(p => p.Symbol).HasMaxLength(16).IsRequired();
                b.HasIndex(p => p.Symbol);
            });

            #endregion
            #region CorporateAction

            modelBuilder.Entity<CorporateAction>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(22).IsRequired();
                b.Property(p => p.Symbol).HasMaxLength(16).IsRequired();
                b.Property(p => p.EventName).HasMaxLength(256);
                b.Property(p => p.EventTitle).HasMaxLength(256);
                b.Property(p => p.Exchange).HasMaxLength(16);
                b.Property(p => p.EventListCode).HasMaxLength(128);
                b.Property(p => p.EventCode).HasMaxLength(16);
                b.HasIndex(p => p.EventCode);
                b.HasIndex(p => new { p.EventCode, p.Symbol, p.Exchange });
            });

            #endregion
            #region FiinEvaluated

            modelBuilder.Entity<FiinEvaluated>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(22).IsRequired();
                b.Property(p => p.Symbol).HasMaxLength(16).IsRequired();
                b.HasIndex(p => p.Symbol);
            });

            #endregion
            #region StockRecommendation

            modelBuilder.Entity<StockRecommendation>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(22).IsRequired();
                b.Property(p => p.Symbol).HasMaxLength(16).IsRequired();
                b.Property(p => p.Firm).HasMaxLength(64);
                b.Property(p => p.Type).HasMaxLength(16);
                b.Property(p => p.Analyst).HasMaxLength(64);
                b.Property(p => p.Source).HasMaxLength(64);
                b.HasIndex(p => new { p.Symbol, p.Analyst, p.ReportDate });
            });

            #endregion
            #region VndStockScore

            modelBuilder.Entity<VndStockScore>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(22).IsRequired();
                b.Property(p => p.Symbol).HasMaxLength(16).IsRequired();
                b.HasIndex(p => p.Symbol);
                b.Property(p => p.Type).HasMaxLength(16);
                b.Property(p => p.ModelCode).HasMaxLength(16);
                b.Property(p => p.CriteriaCode).HasMaxLength(16);
                b.Property(p => p.CriteriaType).HasMaxLength(32);
                b.Property(p => p.CriteriaName).HasMaxLength(256);
                b.Property(p => p.CriteriaType).HasMaxLength(32);
                b.Property(p => p.Locale).HasMaxLength(8);
            });

            #endregion
            #region StockTransaction

            modelBuilder.Entity<StockTransaction>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(22).IsRequired();
                b.Property(p => p.Symbol).HasMaxLength(16).IsRequired();
                b.HasIndex(p => new { p.Symbol, p.TradingDate });
            });

            #endregion
        }
    }
}