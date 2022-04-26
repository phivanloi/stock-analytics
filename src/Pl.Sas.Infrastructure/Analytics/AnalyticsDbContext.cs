using Microsoft.EntityFrameworkCore;
using Pl.Sas.Core.Entities;

namespace Pl.Sas.Infrastructure.Analytics
{
    public class AnalyticsDbContext : DbContext
    {
        public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options) { }

        public virtual DbSet<AnalyticsResult> AnalyticsResults { get; set; }

        public virtual DbSet<TradingResult> TradingResults { get; set; }

        public virtual DbSet<StockFeature> StockFeatures { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Schedule

            modelBuilder.Entity<Schedule>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(22);
                b.Property(p => p.Name).HasMaxLength(128);
                b.Property(p => p.DataKey).HasMaxLength(64);
                b.HasIndex(p => p.Activated);
                b.HasIndex(p => p.ActiveTime);
                b.HasIndex(p => p.Priority);
            });

            #endregion Schedule

            #region AnalyticsResult

            modelBuilder.Entity<AnalyticsResult>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(22);
                b.Property(p => p.Symbol).HasMaxLength(16).IsRequired();
                b.HasIndex(p => p.Symbol);
                b.Property(p => p.DatePath).HasMaxLength(8).IsRequired();
                b.HasIndex(p => p.DatePath);
                b.HasIndex(p => new { p.Symbol, p.DatePath });
                b.Property(p => p.SsaPerdictPrice).HasColumnType("decimal(35, 10)");
                b.Property(p => p.FttPerdictPrice).HasColumnType("decimal(35, 10)");
                b.Property(p => p.TargetPrice).HasColumnType("decimal(35, 10)");
            });

            #endregion AnalyticsResult

            #region TradingResult

            modelBuilder.Entity<TradingResult>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(22);
                b.Property(p => p.Symbol).HasMaxLength(16).IsRequired();
                b.HasIndex(p => p.Symbol);
                b.Property(p => p.DatePath).HasMaxLength(8).IsRequired();
                b.HasIndex(p => p.DatePath);
                b.HasIndex(p => p.Principle);
                b.HasIndex(p => new { p.Symbol, p.DatePath });
                b.HasIndex(p => new { p.Symbol, p.DatePath, p.Principle });
                b.Property(p => p.BuyPrice).HasColumnType("decimal(10, 4)");
                b.Property(p => p.SellPrice).HasColumnType("decimal(10, 4)");
                b.Property(p => p.Profit).HasColumnType("decimal(35, 10)");
                b.Property(p => p.Capital).HasColumnType("decimal(35, 10)");
                b.Property(p => p.TotalTax).HasColumnType("decimal(35, 10)");
                b.Property(p => p.ProfitPercent).HasColumnType("decimal(35, 10)");
            });

            #endregion TradingResult

            #region StockFeature

            modelBuilder.Entity<StockFeature>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(22);
                b.Property(p => p.Symbol).HasMaxLength(16).IsRequired();
                b.HasIndex(p => p.Symbol);
            });

            #endregion StockTransaction
        }
    }
}