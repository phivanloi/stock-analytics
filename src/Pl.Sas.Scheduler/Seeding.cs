using Pl.Sas.Core.Entities;
using Pl.Sas.Infrastructure.Data;
using Pl.Sas.Infrastructure.System;
using System.Text.Json;

namespace Pl.Sas.Scheduler
{
    public static class Seeding
    {
        public static IServiceProvider SystemDbSeed(this IServiceProvider serviceProvider, SystemDbContext systemDbContext)
        {
            if (!systemDbContext.Schedules.Any())
            {
                var schedules = new List<Schedule>
                {
                    new Schedule()
                    {
                        Type = 0,
                        Name = "Tìm kiếm và bổ sung mã chứng khoáng từ ssi api",
                    },
                    new Schedule()
                    {
                        Type = 50,
                        Name = "Lấy lãi suất ngân hàng cao nhất.",
                        OptionsJson = JsonSerializer.Serialize(new Dictionary<string, string>() { { "Length", $"3,6,12,24" } })
                    },

                    new Schedule()
                    {
                        Type = 100,
                        Name = "Đánh giá mã ngành tự động.",
                        ActiveTime = DateTime.Now.AddHours(1)
                    },
                    new Schedule()
                    {
                        Type = 200,
                        Name = "Xử lý ngày giao dịch không hưởng quyền chi trả cổ tức thì xóa và import lại lịch sử giá.",
                        ActiveTime = DateTime.Now.Date.AddDays(1).AddHours(2).AddMinutes(50)
                    },

                    new Schedule()
                    {
                        Type = 300,
                        Name = "Xử lý hiển thị dữ liệu chứng khoán cho hiển thị.",
                        ActiveTime = DateTime.Now.AddMinutes(60)
                    }
                };
                systemDbContext.Schedules.AddRange(schedules);
                systemDbContext.SaveChanges();
            }
            return serviceProvider;
        }
    }
}
