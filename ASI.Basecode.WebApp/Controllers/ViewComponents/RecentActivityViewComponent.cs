using Microsoft.AspNetCore.Mvc;

namespace ASI.Basecode.WebApp.Controllers.ViewComponents
{
    /// <summary>
    /// View Component for rendering a list of recent activities in the Teacher dashboard.
    /// This component displays a styled list similar to the Admin dashboard's recent activity section.
    /// </summary>
    public class RecentActivityViewComponent : ViewComponent
    {
        /// <summary>
        /// Invokes the Recent Activity View Component.
        /// </summary>
        /// <returns>An IViewComponentResult containing the rendered recent activity list view.</returns>
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
