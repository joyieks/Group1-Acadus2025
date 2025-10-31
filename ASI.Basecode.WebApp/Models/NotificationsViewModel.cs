using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    /// <summary>
    /// View model containing notification items for display in a dropdown or page.
    /// </summary>
    public class NotificationsViewModel
    {
        /// <summary>List of notifications to show to the user.</summary>
        public List<NotificationItem> Notifications { get; set; } = new();

        /// <summary>Indicates whether the model contains any notifications.</summary>
        public bool HasData => Notifications != null && Notifications.Count > 0;

        /// <summary>Represents a single notification entry.</summary>
        public class NotificationItem
        {
            /// <summary>Short title of the notification.</summary>
            public string Title { get; set; }

            /// <summary>Detailed message text.</summary>
            public string Message { get; set; }

            /// <summary>Timestamp when the notification was created.</summary>
            public DateTime Date { get; set; }

            /// <summary>Whether the notification has been read by the user.</summary>
            public bool IsRead { get; set; }
        }
    }
}