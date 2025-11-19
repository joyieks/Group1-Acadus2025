using ASI.Basecode.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers.ViewComponents
{
    /// <summary>
    /// ViewComponent for displaying the top 5 students in a course.
    /// </summary>
    public class TopStudentsViewComponent : ViewComponent
    {
        private readonly ITeacherCourseService _teacherCourseService;

        public TopStudentsViewComponent(ITeacherCourseService teacherCourseService)
        {
            _teacherCourseService = teacherCourseService;
        }

        /// <summary>
        /// Invokes the TopStudents ViewComponent to render the top students table.
        /// </summary>
        /// <param name="courseId">The course ID to get top students for.</param>
        /// <returns>The ViewComponent result containing the top students table.</returns>
        public async Task<IViewComponentResult> InvokeAsync(int? courseId)
        {
            var topStudents = new List<TopStudentViewModel>();

            if (courseId.HasValue && courseId.Value > 0)
            {
                try
                {
                    // Get course gradebook data which includes student averages
                    var gradebook = await _teacherCourseService.GetCourseGradebookAsync(courseId.Value);

                    if (gradebook != null && gradebook.Students != null && gradebook.Students.Any())
                    {
                        // Sort students by average percentage (descending) and take top 5
                        var topPerformers = gradebook.Students
                            .Where(s => s.AveragePercentage > 0) // Only include students with graded activities
                            .OrderByDescending(s => s.AveragePercentage)
                            .Take(5)
                            .ToList();

                        foreach (var student in topPerformers)
                        {
                            // Split name into first and last name
                            var nameParts = student.Name.Split(new[] { ' ' }, 2, System.StringSplitOptions.RemoveEmptyEntries);
                            var firstName = nameParts.Length > 0 ? nameParts[0] : "";
                            var lastName = nameParts.Length > 1 ? nameParts[1] : "";

                            topStudents.Add(new TopStudentViewModel
                            {
                                IdNumber = student.StudentDisplayId ?? "N/A",
                                FirstName = firstName,
                                LastName = lastName,
                                Grade = student.AveragePercentage.ToString("0.0") + "%" // Format as percentage
                            });
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    // Log error but don't crash - return empty list
                    System.Console.WriteLine($"Error loading top students for course {courseId}: {ex.Message}");
                }
            }

            // If no students found, return empty list (view will handle empty state)
            return View(topStudents);
        }
    }

    /// <summary>
    /// ViewModel for a top student.
    /// </summary>
    public class TopStudentViewModel
    {
        /// <summary>
        /// Gets or sets the student's ID number.
        /// </summary>
        public string IdNumber { get; set; }

        /// <summary>
        /// Gets or sets the student's first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the student's last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the student's grade.
        /// </summary>
        public string Grade { get; set; }
    }
}
