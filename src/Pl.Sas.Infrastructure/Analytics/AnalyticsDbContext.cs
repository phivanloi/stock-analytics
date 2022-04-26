using Microsoft.EntityFrameworkCore;
using Pl.Sas.Core.Entities;

namespace Pl.Sas.Infrastructure.Analytics
{
    public class AnalyticsDbContext : DbContext
    {
        public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options) { }

        public virtual DbSet<AnalyticsResult> AnalyticsResults { get; set; } = null!;
        public virtual DbSet<TradingResult> TradingResults { get; set; } = null!;
        public virtual DbSet<StockTracking> StockTrackings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region AnalyticsResult

            modelBuilder.Entity<AnalyticsResult>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(22).IsRequired();
                b.Property(p => p.Symbol).HasMaxLength(16).IsRequired();
                b.HasIndex(p => new { p.Symbol, p.TradingDate });
            });

            #endregion AnalyticsResult

            #region TradingResult

            modelBuilder.Entity<TradingResult>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(22).IsRequired();
                b.Property(p => p.Symbol).HasMaxLength(16).IsRequired();
                b.HasIndex(p => new { p.Symbol, p.TradingDate, p.Principle });
            });

            #endregion TradingResult

            #region StockTracking

            modelBuilder.Entity<StockTracking>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(22).IsRequired();
                b.Property(p => p.Symbol).HasMaxLength(16).IsRequired();
                b.Property(p => p.DownloadStatus).HasMaxLength(1024);
                b.Property(p => p.DataStatus).HasMaxLength(1024);
                b.Property(p => p.AnalyticsStatus).HasMaxLength(1024);
                b.HasIndex(p => new { p.Symbol });
            });

            #endregion StockTracking
        }
    }
}