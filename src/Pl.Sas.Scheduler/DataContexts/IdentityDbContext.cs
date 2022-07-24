using Microsoft.EntityFrameworkCore;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Entities.Identity;

namespace Pl.Sas.Scheduler.DataContexts
{
    public class IdentityDbContext : DbContext
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

        public virtual DbSet<FollowStock> FollowStocks { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region FollowStock
            builder.Entity<FollowStock>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(22).IsRequired();
                b.Property(p => p.UserId).HasMaxLength(22).IsRequired();
                b.Property(p => p.Symbol).HasMaxLength(16).IsRequired();
                b.HasIndex(p => new { p.UserId, p.Symbol });
            });
            #endregion

            #region User
            builder.Entity<User>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(22).IsRequired();
                b.Property(p => p.UserName).HasMaxLength(128).IsRequired();
                b.Property(p => p.Email).HasMaxLength(128);
                b.Property(p => p.Phone).HasMaxLength(16);
                b.Property(p => p.Password).HasMaxLength(512);
                b.Property(p => p.Avatar).HasMaxLength(1024);
                b.Property(p => p.FullName).HasMaxLength(64).IsRequired();
                b.HasIndex(p => new { p.UserName });
            });
            #endregion
        }
    }
}