using System;
using System.Collections.Generic;

namespace Pl.Sas.WebDashboard.Models
{
    public class NotificationViewModel
    {
        public List<UserNotificationViewModel> Notifications { get; set; } = new List<UserNotificationViewModel>();
    }

    public class UserNotificationViewModel
    {
        public string Id { get; set; }

        public DateTime CreatedTime { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string Symbol { get; set; }
    }
}