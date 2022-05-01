using Microsoft.EntityFrameworkCore;
using Pl.Sas.Core.Entities;

namespace Pl.Sas.Infrastructure.System
{
    public class SystemDbContext : DbContext
    {
        public SystemDbContext(DbContextOptions<SystemDbContext> options) : base(options) { }

        public virtual DbSet<Schedule> Schedules { get; set; } = null!;
        public virtual DbSet<KeyValue> KeyValues { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Schedule

            modelBuilder.Entity<Schedule>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(22).IsRequired();
                b.Property(p => p.Name).HasMaxLength(128);
                b.Property(p => p.DataKey).HasMaxLength(64);
                b.HasIndex(p => p.ActiveTime);
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