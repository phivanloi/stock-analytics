using Pl.Sas.Core.Entities;
using Pl.Sas.Infrastructure.Data;

namespace Pl.Sas.Scheduler
{
    public static class Seeding
    {
        public static IServiceProvider MarketSeed(this IServiceProvider serviceProvider, MarketDbContext marketDbContext)
        {
            if (!marketDbContext.Schedules.Any())
            {
                marketDbContext.Schedules.Add(new Schedule()
                {
                    Name = "Tìm kiếm và bổ sung mã chứng khoáng vào trong hệ thống.",
                    Type = 0
                });
                marketDbContext.Schedules.Add(new Schedule()
                {
                    Name = "Phân tích dòng tiền theo ngành",
                    Type = 1
                });

                marketDbContext.SaveChanges();
            }

            return serviceProvider;
        }
    }
}
