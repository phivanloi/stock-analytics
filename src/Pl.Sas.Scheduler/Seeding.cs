using Pl.Sas.Core.Entities;
using Pl.Sas.Core.Entities.Security;
using Pl.Sas.Infrastructure.Identity;
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
                        Type = 10,
                        Name = "Lấy lãi suất ngân hàng cao nhất.",
                        OptionsJson = JsonSerializer.Serialize(new Dictionary<string, string>() { { "Length", $"3,6,12,24" } })
                    },

                    new Schedule()
                    {
                        Type = 204,
                        Name = "Đánh giá mã ngành tự động.",
                        ActiveTime = DateTime.Now.AddMinutes(15)
                    },
                    new Schedule()
                    {
                        Type = 205,
                        Name = "Xử lý ngày giao dịch không hưởng quyền chi trả cổ tức thì xóa và import lại lịch sử giá.",
                        ActiveTime = DateTime.Now.Date.AddDays(1).AddHours(2).AddMinutes(50)
                    },
                };
                systemDbContext.Schedules.AddRange(schedules);
                systemDbContext.SaveChanges();
            }
            return serviceProvider;
        }

        public static IServiceProvider IdentitySeed(this IServiceProvider serviceProvider, IdentityDbContext identityDbContext)
        {
            if (!identityDbContext.Users.Any(q => q.UserName == "phivanloi@gmail.com"))
            {
                User user = new()
                {
                    UserName = "phivanloi@gmail.com",
                    Email = "phivanloi@gmail.com",
                    Active = true,
                    Deleted = false,
                    FullName = "Quản trị viên",
                    Avatar = "http://s120-ava-talk.zadn.vn/a/4/2/3/1/120/e874de0ca7c1ac55ddb4c1e047e463d8.jpg",
                    Phone = "0906282026",
                    IsAdministator = true,
                    DateOfBirth = new DateTime(1989, 9, 28),
                    Password = Cryptography.CreateMd5Password("Liemtinmoi@2413")
                };
                identityDbContext.Add(user);
                identityDbContext.SaveChanges(true);
            }
            return serviceProvider;
        }
    }
}
