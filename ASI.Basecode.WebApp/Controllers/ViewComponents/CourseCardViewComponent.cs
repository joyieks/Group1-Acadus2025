using Microsoft.AspNetCore.Mvc;
using ASI.Basecode.WebApp.Models;

/// <summary>
/// View component for rendering a course card.
/// </summary>
public class CourseCardViewComponent : ViewComponent
{
    /// <summary>
    /// Invokes the view component with the specified course model.
    /// </summary>
    /// <param name="course">The course view model.</param>
    /// <returns>The view component result.</returns>
    public IViewComponentResult Invoke(TeacherCourseViewModel course)
    {
        return View(course);
    }
}
