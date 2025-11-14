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

        public async Task<List<ActivityModel>> GetActivitiesByCourseIdAsync(int courseId)
        {
            var res = await _supabaseClient
                .From<ActivityModel>()
                .Where(a => a.CourseId == courseId)
                .Where(a => a.IsArchived == false)
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



        public async Task<List<ActivitySubmissionModel>> GetSubmissionsByStudentAndCourseAsync(string studentId, int courseId)
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
                        if (activity == null || activity.maxScore <= 0) return 0.0;

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



    }
}
