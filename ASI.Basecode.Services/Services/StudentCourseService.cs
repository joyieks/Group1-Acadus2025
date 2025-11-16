using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ASI.Basecode.Services.Services
{
    public class StudentCourseService : IStudentCourseService
    {
        private readonly IStudentCourseRepository _studentCourseRepository;

        public StudentCourseService(IStudentCourseRepository studentCourseRepository)
        {
            _studentCourseRepository = studentCourseRepository;
        }

        public async Task<List<CourseModel>> GetCoursesByStudentAsync(string studentId)
        {
            if (string.IsNullOrWhiteSpace(studentId))
                throw new System.ArgumentException("Invalid Student ID.");

            var courses = await _studentCourseRepository.GetCoursesByStudentIdAsync(studentId);
            return courses ?? new List<CourseModel>();
        }

        public async Task<StudentCourseDetailsViewModel> GetCourseDetailsAsync(string studentId, string courseId)
        {
            int cid = int.Parse(courseId);

            // 1. Fetch course
            var allCourses = await _studentCourseRepository.GetCoursesByStudentIdAsync(studentId);
            var course = allCourses.FirstOrDefault(c => c.Id == cid);

            // 2. Fetch instructor
            SupabaseUserNew instructor = null;
            if (course != null && !string.IsNullOrWhiteSpace(course.TeacherId))
            {
                instructor = await _studentCourseRepository.GetUserByUserTypeIdAsync(course.TeacherId);
            }

            // 3. Fetch activities and submissions
            var activities = await _studentCourseRepository.GetActivitiesByCourseIdAsync(cid);
            var submissions = await _studentCourseRepository.GetSubmissionsByStudentAndCourseAsync(studentId, cid);

            // 4. Map activities
            var activityItems = activities.Select(a =>
            {
                var sub = submissions.FirstOrDefault(s => s.ActivityId == a.Id);

                double? percentage = null;
                if (sub != null && a.maxScore > 0)
                {
                    percentage = Math.Round((double)sub.Score / a.maxScore * 100, 1);
                }

                return new StudentCourseDetailsViewModel.ActivityItem
                {
                    Title = a.Title,
                    Description = a.Description,
                    DueDate = a.DueDate.ToString("yyyy-MM-dd"),
                    Status = sub?.SubmissionStatus ?? "Not Submitted",
                    Score = sub?.Score.ToString() ?? "0",
                    Date = a.DueDate,
                    CanAppeal = false,
                    Percentage = percentage?.ToString("0.0") ?? "0.0"
                };

            }).ToList();

            // 5. Map feedbacks with instructor name
            var instructorName = instructor != null
                ? $"{instructor.FirstName} {instructor.LastName}".Trim()
                : "";

            var feedbackItems = submissions
                .Where(s => !string.IsNullOrWhiteSpace(s.Feedback))
                .Select(s => new StudentCourseDetailsViewModel.FeedbackItem
                {
                    Title = "Feedback for Activity " + s.ActivityId,
                    Date = s.CreatedAt.ToString("yyyy-MM-dd"),
                    Content = s.Feedback,
                    Instructor = instructorName,
                    DateGiven = s.CreatedAt
                }).ToList();

            // 6. Return view model
                var model = new StudentCourseDetailsViewModel
                {
                    CourseId = courseId,
                    CourseTitle = course?.Name ?? $"Course {courseId}",
                    UserName = "First Name",
                    Activities = activityItems,
                    Feedbacks = feedbackItems,

                    TotalTasks = activityItems.Count,
                    CompletedTasks = activityItems.Count(a => a.Status == "Graded" || a.Status == "Completed"),
                    PendingTasks = activityItems.Count(a => a.Status != "Graded" && a.Status != "Completed"),

                    CurrentPage = 1,
                    TotalPages = 1,
                    CurrentTab = "grades"
                };

                // ✅ Set GPA using helper method
                model.OverallGPA = model.GetCourseAverage();

                return model;

        }

        public async Task<List<StudentReportViewModel.ReportItem>> GetStudentReportsAsync(string studentId)
        {
            return await _studentCourseRepository.GetStudentCourseReportsAsync(studentId);
        }

        public async Task<StudentDashboardViewModel> GetStudentDashboardAsync(string studentId)
        {
            return await _studentCourseRepository.GetStudentDashboardAsync(studentId);
        }
    }
}
