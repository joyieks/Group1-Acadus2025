using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Controllers.ViewComponents
{
    /// <summary>
    /// ViewComponent for displaying the top 5 students in a course.
    /// </summary>
    public class TopStudentsViewComponent : ViewComponent
    {
        /// <summary>
        /// Invokes the TopStudents ViewComponent to render the top students table.
        /// </summary>
        /// <returns>The ViewComponent result containing the top students table.</returns>
        public IViewComponentResult Invoke()
        {
            // Fallback sample data for top students
            var topStudents = new List<TopStudentViewModel>
            {
                new TopStudentViewModel { IdNumber = "123456", FirstName = "John", LastName = "Doe", Grade = "95" },
                new TopStudentViewModel { IdNumber = "123457", FirstName = "Jane", LastName = "Smith", Grade = "92" },
                new TopStudentViewModel { IdNumber = "123458", FirstName = "Alice", LastName = "Johnson", Grade = "89" },
                new TopStudentViewModel { IdNumber = "123459", FirstName = "Bob", LastName = "Williams", Grade = "87" },
                new TopStudentViewModel { IdNumber = "123460", FirstName = "Charlie", LastName = "Brown", Grade = "85" }
            };

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
