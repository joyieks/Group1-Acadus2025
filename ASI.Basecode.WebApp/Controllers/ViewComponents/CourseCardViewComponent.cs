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
        // Map to CourseCardViewModel for the view
        var cardModel = new CourseCardViewModel
        {
            Id = course.Id,
            CourseCode = course.CourseCode,
            CourseTitle = course.CourseTitle,
            SemesterInfo = course.SemesterInfo,
            CardColor = course.CardColor
        };
        return View(cardModel);
    }
}
