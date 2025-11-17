using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Supabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace ASI.Basecode.Data.Repositories
{
    public class StudentCourseRepository : IStudentCourseRepository
    {
        private readonly Client _supabaseClient;

        public StudentCourseRepository(Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<List<CourseModel>> GetCoursesByStudentIdAsync(string studentId)
        {
            if (string.IsNullOrWhiteSpace(studentId))
                return new List<CourseModel>();

            // 1. Get active enrollments for this student
            var enrollmentsResponse = await _supabaseClient
                .From<EnrollmentModel>()
                .Where(e => e.StudentId == studentId && e.Status == "Active")
                .Get();

            var enrolledCourseIds = enrollmentsResponse.Models.Select(e => e.CourseId).ToList();

            if (!enrolledCourseIds.Any())
                return new List<CourseModel>();

            // 2. Fetch all courses and filter in memory
            var allCoursesResponse = await _supabaseClient
                .From<CourseModel>()
                .Get();

            var enrolledCourses = allCoursesResponse.Models
                .Where(c => enrolledCourseIds.Contains(c.Id))
                .ToList();

            return enrolledCourses;
        }

        public async Task<List<ActivityModel>> GetActivitiesByCourseIdAsync(long courseId)
        {
            var res = await _supabaseClient
                .From<ActivityModel>()
                .Where(a => a.CourseId == courseId)
                .Where(a => a.IsVisible == false)
                .Get();

            return res.Models
                .OrderBy(a => a.DueDate)
                .ToList();
        }

        public async Task<SupabaseUserNew> GetUserByUserTypeIdAsync(string userTypeId)
        {
            var res = await _supabaseClient
                .From<SupabaseUserNew>()
                .Where(u => u.UserTypeId == userTypeId)
                .Get();

            return res.Models.FirstOrDefault();
        }



        public async Task<List<ActivitySubmissionModel>> GetSubmissionsByStudentAndCourseAsync(string studentId, long courseId)
        {
            var activities = await GetActivitiesByCourseIdAsync(courseId);
            var activityIds = activities.Select(a => a.Id).ToList();

            if (!activityIds.Any())
                return new List<ActivitySubmissionModel>();

            var res = await _supabaseClient
                .From<ActivitySubmissionModel>()
                .Where(s => s.StudentId == studentId)
                .Filter("activityId", Supabase.Postgrest.Constants.Operator.In, activityIds.ToArray())
                .Get();

            return res.Models;
        }

        public async Task<List<StudentReportViewModel.ReportItem>> GetStudentCourseReportsAsync(string studentId)
        {
            var courses = await GetCoursesByStudentIdAsync(studentId);
            var reportItems = new List<StudentReportViewModel.ReportItem>();

            foreach (var course in courses)
            {
                var activities = await GetActivitiesByCourseIdAsync(course.Id);
                var submissions = await GetSubmissionsByStudentAndCourseAsync(studentId, course.Id);

                var graded = submissions
                    .Where(s => s.SubmissionStatus == "Graded")
                    .Select(s =>
                    {
                        var activity = activities.FirstOrDefault(a => a.Id == s.ActivityId);
                        if (activity == null || activity.maxScore <= 0) 
                            return 0.0;

                        var percentage = (double)s.Score / activity.maxScore * 100;
                        return percentage;
                    })
                    .Where(p => p > 0)
                    .ToList();

                var average = graded.Any() ? Math.Round(graded.Average(), 1) : 0;

                reportItems.Add(new StudentReportViewModel.ReportItem
                {
                    CourseCode = course.Code,
                    CourseTitle = course.Name,
                    MidtermGrade = ConvertPercentageToGPA(average),  // or split by term if needed
                    FinalGrade = 0,
                });
            }

            return reportItems;
        }

        private double ConvertPercentageToGPA(double percentage)
        {
            if (percentage >= 98) return 1.00;
            if (percentage >= 95) return 1.25;
            if (percentage >= 92) return 1.50;
            if (percentage >= 89) return 1.75;
            if (percentage >= 86) return 2.00;
            if (percentage >= 83) return 2.25;
            if (percentage >= 80) return 2.50;
            if (percentage >= 78) return 2.75;
            if (percentage >= 75) return 3.00;
            return 5.00;
        }

        public async Task<StudentDashboardViewModel> GetStudentDashboardAsync(string studentId)
        {
            var dashboard = new StudentDashboardViewModel
            {
                UserName = "First Name", // Optional: fetch from user table if needed
                RecentlyGradedTasks = new List<StudentDashboardViewModel.TaskItem>(),
                ToBeGradedTasks = new List<StudentDashboardViewModel.TaskItem>()
            };

            var courses = await GetCoursesByStudentIdAsync(studentId);
            foreach (var course in courses)
            {
                var activities = await GetActivitiesByCourseIdAsync(course.Id);
                var submissions = await GetSubmissionsByStudentAndCourseAsync(studentId, course.Id);

                foreach (var submission in submissions)
                {
                    var activity = activities.FirstOrDefault(a => a.Id == submission.ActivityId);
                    if (activity == null) continue;

                    var task = new StudentDashboardViewModel.TaskItem
                    {
                        Title = activity.Title,
                        UserAction = submission.SubmissionStatus,
                        Score = submission.SubmissionStatus == "Graded" ? submission.Score.ToString() : null,
                        DueDate = activity.DueDate,
                        Priority = GetPriority(activity.DueDate),
                        StudentId = int.TryParse(studentId, out var sid) ? sid : null,
                        CourseId = activity.CourseId,
                        CourseCode = course.Code

                    };

                    if (submission.SubmissionStatus == "Graded")
                        dashboard.RecentlyGradedTasks.Add(task);
                    else
                        dashboard.ToBeGradedTasks.Add(task);
                }
            }

            return dashboard;
        }

        private string GetPriority(DateTime dueDate)
        {
            var daysLeft = (dueDate - DateTime.Now).TotalDays;
            if (daysLeft <= 2) return "High";
            if (daysLeft <= 5) return "Normal";
            return "Low";
        }



    }
}
