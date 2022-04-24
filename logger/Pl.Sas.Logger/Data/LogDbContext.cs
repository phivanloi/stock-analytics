using Microsoft.EntityFrameworkCore;

namespace Pl.Sas.Logger.Data
{
    public class LogDbContext : DbContext
    {
        public LogDbContext(DbContextOptions<LogDbContext> options) : base(options) { }

        public virtual DbSet<LogEntry> LogEntries { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LogEntry>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(22);
                b.Property(p => p.Host).HasMaxLength(64);
                b.Property(p => p.Message).HasMaxLength(256);
                b.HasIndex(p => p.CreatedTime);
                b.HasIndex(p => p.Type);
                b.HasIndex(p => new { p.Type, p.CreatedTime });
            });
        }
    }
}