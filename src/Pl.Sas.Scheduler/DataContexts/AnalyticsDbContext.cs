using Microsoft.EntityFrameworkCore;
using Pl.Sas.Core.Entities;

namespace Pl.Sas.Scheduler.DataContexts
{
    public class AnalyticsDbContext : DbContext
    {
        public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options) { }

        public virtual DbSet<AnalyticsResult> AnalyticsResults { get; set; } = null!;//bảng kết quản phân tích cổ phiếu
        public virtual DbSet<TradingResult> TradingResults { get; set; } = null!;//bảng kết quả giao dịch
        public virtual DbSet<KeyValue> KeyValues { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region AnalyticsResult

            modelBuilder.Entity<AnalyticsResult>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(22).IsRequired();
                b.Property(p => p.Symbol).HasMaxLength(16).IsRequired();
                b.HasIndex(p => new { p.Symbol });
            });

            #endregion
            #region TradingResult

            modelBuilder.Entity<TradingResult>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(22).IsRequired();
                b.Property(p => p.Symbol).HasMaxLength(16).IsRequired();
                b.HasIndex(p => new { p.Symbol, p.Principle });
            });

            #endregion
            #region KeyValue
            modelBuilder.Entity<KeyValue>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(22).IsRequired();
                b.Property(p => p.Key).HasMaxLength(128).IsRequired();
                b.Property(p => p.Value).IsRequired();
                b.HasIndex(p => new { p.Key });
            });
            #endregion
        }
    }
}