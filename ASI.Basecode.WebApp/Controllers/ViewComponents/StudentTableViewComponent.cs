using Microsoft.AspNetCore.Mvc;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers.ViewComponents
{
    /// <summary>
    /// ViewComponent for displaying a paginated table of students in a course.
    /// </summary>
    public class StudentTableViewComponent : ViewComponent
    {
        private readonly ISupabaseAuthService _supabaseAuthService;
        private readonly IUserService _userService;

        public StudentTableViewComponent(ISupabaseAuthService supabaseAuthService, IUserService userService)
        {
            _supabaseAuthService = supabaseAuthService;
            _userService = userService;
        }

        /// <summary>
        /// Invokes the StudentTable ViewComponent to render the student table.
        /// </summary>
        /// <param name="courseId">The course ID to get enrolled students for.</param>
        /// <returns>The ViewComponent result containing the student table.</returns>
        public async Task<IViewComponentResult> InvokeAsync(int? courseId)
        {
            var students = new List<StudentViewModel>();

            if (courseId.HasValue)
            {
                try
                {
                    var client = await _supabaseAuthService.GetSupabaseClientForAuthAsync();

                    // Get all enrollments for this course (filter active in memory to avoid PostgREST issues)
                    var enrollmentsResponse = await client
                        .From<EnrollmentModel>()
                        .Filter("course_id", Supabase.Postgrest.Constants.Operator.Equals, courseId.Value)
                        .Get();
                    
                    // Filter for active status in memory (check for "Active" enum value)
                    var allEnrollments = enrollmentsResponse?.Models ?? new List<EnrollmentModel>();
                    var enrollments = allEnrollments
                        .Where(e => !string.IsNullOrEmpty(e.Status) && 
                                   (e.Status == "Active" || e.Status.Equals("active", StringComparison.OrdinalIgnoreCase)))
                        .ToList();

                    // Get all students (users with student role)
                    var allStudents = await _userService.GetStudentsAsync();

                    // Map enrollments to student view models
                    foreach (var enrollment in enrollments)
                    {
                        var student = allStudents.FirstOrDefault(s => s.UserTypeId == enrollment.StudentId);
                        if (student != null)
                        {
                            students.Add(new StudentViewModel
                            {
                                IdNumber = student.UserDisplayId ?? "N/A",
                                FirstName = student.FirstName ?? "",
                                LastName = student.LastName ?? "",
                                Status = enrollment.Status ?? "Active",
                                StudentId = student.UserTypeId
                            });
                        }
                    }

                    Console.WriteLine($"StudentTableViewComponent: Found {students.Count} enrolled students for course {courseId}");
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"Error loading enrolled students: {ex.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                }
            }

            return View(students);
        }
    }

    /// <summary>
    /// ViewModel for a student in the table.
    /// </summary>
    public class StudentViewModel
    {
        /// <summary>
        /// Gets or sets the student's ID number (UserDisplayId).
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

        /// <summary>
        /// Gets or sets the enrollment status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the student's UserTypeId (UUID).
        /// </summary>
        public string StudentId { get; set; }
    }
}
