using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.WebApp.Models
{
    /// <summary>
    /// View model for a teacher's course card.
    /// </summary>
    public class TeacherCourseViewModel
    {
        /// <summary>
        /// Gets or sets the course code.
        /// </summary>
        [Required]
        public string CourseCode { get; set; }

        /// <summary>
        /// Gets or sets the course title.
        /// </summary>
        [Required]
        public string CourseTitle { get; set; }

        /// <summary>
        /// Gets or sets the semester information.
        /// </summary>
        [Required]
        public string SemesterInfo { get; set; }

        /// <summary>
        /// Gets or sets the background color for the top half of the card.
        /// </summary>
        [Required]
        public string CardColor { get; set; }
    }
}
