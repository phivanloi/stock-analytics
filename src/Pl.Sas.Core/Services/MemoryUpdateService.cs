using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Interfaces;

namespace Pl.Sas.Core.Services
{
    public class MemoryUpdateService : IMemoryUpdateService
    {
        private readonly IMemoryCacheService _memoryCacheService;

        public MemoryUpdateService(
            IMemoryCacheService memoryCacheService)
        {
            _memoryCacheService = memoryCacheService;
        }

        public virtual void HandleUpdateByQueueMessage(QueueMessage queueMessage)
        {
            switch (queueMessage.Id)
            {
                case "Schedule":
                    _memoryCacheService.RemoveByPrefix(Constants.ScheduleCachePrefix);
                    break;
                case "Company":
                    _memoryCacheService.RemoveByPrefix(Constants.CompanyCachePrefix);
                    break;
                case "Stocks":
                    _memoryCacheService.RemoveByPrefix(Constants.StockCachePrefix);
                    break;
            }
        }
    }
}