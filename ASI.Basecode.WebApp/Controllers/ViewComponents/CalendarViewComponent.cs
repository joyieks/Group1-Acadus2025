using Microsoft.AspNetCore.Mvc;

namespace ASI.Basecode.WebApp.Controllers.ViewComponents
{
    /// <summary>
    /// View Component for rendering a calendar placeholder in the Teacher dashboard.
    /// This component displays a simple placeholder for the calendar feature.
    /// </summary>
    public class CalendarViewComponent : ViewComponent
    {
        /// <summary>
        /// Invokes the Calendar View Component.
        /// </summary>
        /// <returns>An IViewComponentResult containing the rendered calendar placeholder view.</returns>
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
