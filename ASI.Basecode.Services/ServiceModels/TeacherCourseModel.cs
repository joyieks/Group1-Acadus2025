using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Service.ServiceModels
{
    /// <summary>
    /// View model for a teacher's course card.
    /// </summary>
    public class TeacherCourseModel
    {
        public int CourseId { get; set; }

        /// Gets or sets the course code.
        [Required]
        public string CourseCode { get; set; }

        /// Gets or sets the course title.
        [Required]
        public string CourseTitle { get; set; }

        /// Gets or sets the semester information.
        [Required]
        public string SemesterInfo { get; set; }

        /// Gets or sets the background color for the top half of the card.
        [Required]
        public string CardColor { get; set; }

        /// Gets or sets the course ID.
        public int Id { get; set; }

        //List of Activities for the course
        public List<TeacherActivityModel> Activities { get; set; } = new List<TeacherActivityModel>();
        public List<TeacherStudentModel> Students { get; set; }
        public List<TeacherActivitySubmissionModel> Submissions { get; set; } = new();

    }
}
