using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models
{
    public class CourseCreateViewModel
    {
        [Required(ErrorMessage = "Course code is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Course code must be between 2 and 50 characters")]
        [Display(Name = "Course Code")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Course name is required")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Course name must be between 3 and 255 characters")]
        [Display(Name = "Course Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Course description is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Course description must be between 10 and 1000 characters")]
        [Display(Name = "Course Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Credits is required")]
        [Range(1, 6, ErrorMessage = "Credits must be between 1 and 6")]
        [Display(Name = "Credits")]
        public long Credits { get; set; }

        [Required(ErrorMessage = "Course level is required")]
        [Display(Name = "Course Level")]
        public string Level { get; set; }

        [Required(ErrorMessage = "Semester is required")]
        [Display(Name = "Semester")]
        public long SemesterId { get; set; }

        [Required(ErrorMessage = "Maximum capacity is required")]
        [Range(1, 500, ErrorMessage = "Maximum capacity must be between 1 and 500")]
        [Display(Name = "Maximum Capacity")]
        public decimal MaxCapacity { get; set; }

        [Required(ErrorMessage = "Instructor is required")]
        [Display(Name = "Instructor")]
        public string InstructorId { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [StringLength(20)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Active";

        // Dropdown lists - populated by controller
        public List<InstructorOption> Instructors { get; set; } = new List<InstructorOption>();
        public List<SemesterOption> Semesters { get; set; } = new List<SemesterOption>();
        public List<LevelOption> Levels { get; set; } = new List<LevelOption>();
    }

    public class InstructorOption
    {
        public string UserTypeId { get; set; }
        public string FullName { get; set; }
    }

    public class SemesterOption
    {
        public long Id { get; set; }
        public string SemesterName { get; set; }
    }

    public class LevelOption
    {
        public string Value { get; set; }
        public string Label { get; set; }
    }
}
