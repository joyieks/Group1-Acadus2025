using Microsoft.AspNetCore.Mvc;
using ASI.Basecode.Webapp.Models;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Controllers.ViewComponents
{
    /// <summary>
    /// ViewComponent for rendering the notifications dropdown in the navbar.
    /// </summary>
    public class NotificationsViewComponent : ViewComponent
    {
        /// <summary>
        /// Invokes the notifications view component.
        /// </summary>
        /// <returns>The view component result.</returns>
        public IViewComponentResult Invoke()
        {
            // For now, return empty model; can be enhanced later
            var model = new NotificationsViewModel
            {
                Notifications = new List<NotificationsViewModel.NotificationItem>()
            };
            return View(model);
        }
    }
}
