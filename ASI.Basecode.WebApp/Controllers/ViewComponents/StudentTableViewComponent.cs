using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Controllers.ViewComponents
{
    /// <summary>
    /// ViewComponent for displaying a paginated table of students in a course.
    /// </summary>
    public class StudentTableViewComponent : ViewComponent
    {
        /// <summary>
        /// Invokes the StudentTable ViewComponent to render the student table.
        /// </summary>
        /// <returns>The ViewComponent result containing the student table.</returns>
        public IViewComponentResult Invoke()
        {
            // Fallback sample data for students
            var students = new List<StudentViewModel>
            {
                new StudentViewModel { IdNumber = "123456", LastName = "Doe", FirstName = "John" },
                new StudentViewModel { IdNumber = "123457", LastName = "Smith", FirstName = "Jane" },
                new StudentViewModel { IdNumber = "123458", LastName = "Johnson", FirstName = "Alice" },
                new StudentViewModel { IdNumber = "123459", LastName = "Williams", FirstName = "Bob" },
                new StudentViewModel { IdNumber = "123460", LastName = "Brown", FirstName = "Charlie" },
                new StudentViewModel { IdNumber = "123461", LastName = "Jones", FirstName = "Diana" },
                new StudentViewModel { IdNumber = "123462", LastName = "Garcia", FirstName = "Eve" },
                new StudentViewModel { IdNumber = "123463", LastName = "Miller", FirstName = "Frank" },
                new StudentViewModel { IdNumber = "123464", LastName = "Davis", FirstName = "Grace" },
                new StudentViewModel { IdNumber = "123465", LastName = "Rodriguez", FirstName = "Henry" },
                new StudentViewModel { IdNumber = "123466", LastName = "Martinez", FirstName = "Ivy" },
                new StudentViewModel { IdNumber = "123467", LastName = "Hernandez", FirstName = "Jack" },
                new StudentViewModel { IdNumber = "123468", LastName = "Lopez", FirstName = "Kate" },
                new StudentViewModel { IdNumber = "123469", LastName = "Gonzalez", FirstName = "Liam" },
                new StudentViewModel { IdNumber = "123470", LastName = "Wilson", FirstName = "Mia" },
                new StudentViewModel { IdNumber = "123471", LastName = "Anderson", FirstName = "Noah" },
                new StudentViewModel { IdNumber = "123472", LastName = "Thomas", FirstName = "Olivia" },
                new StudentViewModel { IdNumber = "123473", LastName = "Taylor", FirstName = "Peter" },
                new StudentViewModel { IdNumber = "123474", LastName = "Moore", FirstName = "Quinn" },
                new StudentViewModel { IdNumber = "123475", LastName = "Jackson", FirstName = "Ryan" }
            };

            return View(students);
        }
    }

    /// <summary>
    /// ViewModel for a student in the table.
    /// </summary>
    public class StudentViewModel
    {
        /// <summary>
        /// Gets or sets the student's ID number.
        /// </summary>
        public string IdNumber { get; set; }

        /// <summary>
        /// Gets or sets the student's last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the student's first name.
        /// </summary>
        public string FirstName { get; set; }
    }
}
