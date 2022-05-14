using Microsoft.EntityFrameworkCore;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;

namespace Pl.Sas.Infrastructure.Identity
{
    /// <summary>
    /// Lớp xử lỹ liệu cho identity
    /// </summary>
    public class IdentityData : IIdentityData
    {
        private readonly IdentityDbContext _identityDbContext;
        private readonly IMemoryCacheService _memoryCacheService;

        public IdentityData(
            IMemoryCacheService memoryCacheService,
            IdentityDbContext identityDbContext)
        {
            _identityDbContext = identityDbContext;
            _memoryCacheService = memoryCacheService;
        }

        public virtual async Task<bool> IsUserHasFollowAsync(string userId)
        {
            return await _identityDbContext.FollowStocks.AnyAsync(f => f.UserId == userId);
        }

        public virtual async Task<List<string>> GetFollowSymbols(string userId)
        {
            return await _identityDbContext.FollowStocks.Where(q => q.UserId == userId).Select(q => q.Symbol).ToListAsync();
        }

        public virtual async Task<bool> ToggleFollow(string userId, string symbol)
        {
            var item = _identityDbContext.FollowStocks.FirstOrDefault(q => q.UserId == userId && q.Symbol == symbol);
            if (item is null)
            {
                _identityDbContext.FollowStocks.Add(new FollowStock()
                {
                    UserId = userId,
                    Symbol = symbol,
                });
            }
            else
            {
                _identityDbContext.FollowStocks.Remove(item);
            }
            return await _identityDbContext.SaveChangesAsync() > 0;
        }
    }
}
