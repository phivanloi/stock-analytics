using Microsoft.EntityFrameworkCore;
using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;
using Pl.Sas.Infrastructure.Analytics;

namespace Pl.Sas.Infrastructure.Identity
{
    /// <summary>
    /// Lớp xử lỹ liệu cho analytic
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


    }
}
