using Microsoft.AspNetCore.Mvc;

namespace ASI.Basecode.WebApp.Controllers.ViewComponents
{
    /// <summary>
    /// ViewComponent for displaying quick actions for a course.
    /// </summary>
    public class QuickActionsViewComponent : ViewComponent
    {
        /// <summary>
        /// Invokes the QuickActions ViewComponent to render the quick actions.
        /// </summary>
        /// <param name="courseId">The ID of the course for which to display quick actions.</param>
        /// <returns>The ViewComponent result containing the quick actions.</returns>
        public IViewComponentResult Invoke(int courseId)
        {
            ViewBag.CourseId = courseId;
            return View();
        }
    }
}
