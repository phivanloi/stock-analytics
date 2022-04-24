﻿using Pl.Sas.Core.Entities;
using Pl.Sas.Infrastructure;

namespace Pl.Sas.Migrations
{
    public static class Seeding
    {
        public static IServiceProvider PlatformSeed(this IServiceProvider serviceProvider, MarketDbContext marketDbContext)
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
                    Name = "Phân tích tâm lý thị trường.",
                    Type = 2,
                    ActiveTime = DateTime.Now.Date.AddHours(10).AddMinutes(1)
                });
                marketDbContext.Schedules.Add(new Schedule()
                {
                    Name = "Tìm kiếm và bổ sung mã chứng khoáng vào trong hệ thống.",
                    Type = 0
                });

                marketDbContext.SaveChanges();
            }

            return serviceProvider;
        }
    }
}
