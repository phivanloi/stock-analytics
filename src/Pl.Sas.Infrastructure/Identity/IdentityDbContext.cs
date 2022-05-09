using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pl.Sas.Core.Entities;

namespace Pl.Sas.Infrastructure.Identity
{
    public class IdentityDbContext : IdentityDbContext<User>
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }


        public virtual DbSet<FollowStock> FollowStocks { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region FollowStock

            builder.Entity<FollowStock>(b =>
            {
                b.Property(c => c.Id).HasMaxLength(36).IsRequired();
                b.Property(p => p.UserId).HasMaxLength(36).IsRequired();
                b.Property(p => p.Symbol).HasMaxLength(36).IsRequired();
                b.HasIndex(p => new { p.UserId, p.Symbol });
            });

            #endregion FollowStock

            builder.Entity<User>(b =>
            {
                b.Property(p => p.Avatar).HasMaxLength(128);
                b.Property(p => p.FullName).HasMaxLength(64);
                b.ToTable("Users");
            });

            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        }
    }
}