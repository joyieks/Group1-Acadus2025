using System;
using System.Collections.Generic;

namespace ASI.Basecode.Webapp.Models
{
    public class NotificationsViewModel
    {
        public List<NotificationItem> Notifications { get; set; } = new();
        public bool HasData => Notifications != null && Notifications.Count > 0;

        public class NotificationItem
        {
            public string Title { get; set; }
            public string Message { get; set; }
            public DateTime Date { get; set; }
            public bool IsRead { get; set; }
        }
    }
}