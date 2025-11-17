using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Supabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ASI.Basecode.Data.Models.CourseGradebookViewModel;

namespace ASI.Basecode.Data.Repositories
{
    public class TeacherCourseRepository : ITeacherCourseRepository
    {
        private readonly Client _supabaseClient;

        public TeacherCourseRepository(Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }
        public async Task<List<ActivityModel>> GetActivitiesByCourseIdAsync(long courseId)
        {
            // Get all activities for the course (filter IsArchived in memory to avoid column not found error)
            var res = await _supabaseClient
                .From<ActivityModel>()
                .Filter("courseId", Supabase.Postgrest.Constants.Operator.Equals, courseId)
                .Get();

            // Filter out archived activities in memory (if IsArchived column exists in model but not in DB, this will handle it)
            var activities = res.Models
                .Where(a => !a.IsArchived)  // Filter in memory
                .OrderBy(a => a.DueDate)
                .ToList();

            return activities;
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
        public async Task<SupabaseUserNew> GetUserByUserTypeIdAsync(string userTypeId)
        {
            var res = await _supabaseClient
                .From<SupabaseUserNew>()
                .Where(u => u.UserTypeId == userTypeId)
                .Get();

            return res.Models.FirstOrDefault();
        }

        public async Task<CourseGradebookViewModel> GetCourseGradebookAsync(int courseId)
        {
            var course = (await _supabaseClient.From<CourseModel>().Where(c => c.Id == courseId).Get()).Models.FirstOrDefault();
            if (course == null) return null;

            var enrollments = (await _supabaseClient.From<EnrollmentModel>().Where(e => e.CourseId == courseId && e.Status == "Active").Get()).Models;
            var activities = await GetActivitiesByCourseIdAsync(courseId);

            var viewModel = new CourseGradebookViewModel
            {
                CourseId = course.Id,
                CourseCode = course.Code,
                CourseTitle = course.Name,
                SemesterInfo = course.SemesterId,
                CardColor = "#E8F9E8" // Optional: dynamic color logic
            };

            foreach (var enrollment in enrollments)
            {
                var submissions = await GetSubmissionsByStudentAndCourseAsync(enrollment.StudentId, courseId);
                var graded = submissions
                    .Where(s => s.SubmissionStatus == "Graded")
                    .Select(s =>
                    {
                        var activity = activities.FirstOrDefault(a => a.Id == s.ActivityId);
                        if (activity == null || activity.maxScore <= 0) return 0.0;
                        return (double)s.Score / activity.maxScore * 100;
                    }).ToList();

                var average = graded.Any() ? Math.Round(graded.Average(), 1) : 0;
        

                var studentUser = await GetUserByUserTypeIdAsync(enrollment.StudentId);
                var fullName = studentUser != null
                    ? $"{studentUser.FirstName} {studentUser.LastName}".Trim()
                    : "Unknown Student";

                viewModel.Students.Add(new CourseGradebookViewModel.StudentGradeItem
                {
                    StudentId = enrollment.StudentId,
                    Name = fullName,
                    AveragePercentage = average
                });

            }

            viewModel.Activities = activities.Select(a => new CourseGradebookViewModel.ActivityGradeItem
            {
                ActivityName = a.Title,
                MaxScore = a.maxScore,
                RawScore = 0 // Optional: aggregate per student if needed
            }).ToList();

            return viewModel;
        }

        public async Task<StudentGradeDetail> GetStudentGradeDetailAsync(string studentId, int courseId)
        {
            var activities = await GetActivitiesByCourseIdAsync(courseId);
            var submissions = await GetSubmissionsByStudentAndCourseAsync(studentId, courseId);
            var user = await GetUserByUserTypeIdAsync(studentId);

            var detail = new StudentGradeDetail
            {
                StudentId = studentId,
                Name = $"{user?.FirstName} {user?.LastName}".Trim()
            };

            foreach (var activity in activities)
            {
                var submission = submissions.FirstOrDefault(s => s.ActivityId == activity.Id && s.SubmissionStatus == "Graded");
                if (submission != null)
                {
                    detail.TotalRawScore += submission.Score;
                    detail.TotalMaxScore += activity.maxScore;

                    detail.Activities.Add(new CourseGradebookViewModel.ActivityGradeItem
                    {
                        ActivityName = activity.Title,
                        ActivityId = activity.Id,
                        RawScore = submission.Score,
                        MaxScore = activity.maxScore
                    });
                }
            }

            
            return detail;
        }

        public async Task<bool> UpdateActivityScoreAsync(string studentId, int activityId, int newScore)
        {
            var res = await _supabaseClient
                .From<ActivitySubmissionModel>()
                .Where(s => s.StudentId == studentId)
                .Where(s => s.ActivityId == activityId)
                .Get();

            var submission = res.Models.FirstOrDefault();
            if (submission == null) return false;

            submission.Score = newScore;
            await submission.Update<ActivitySubmissionModel>();
            return true;
        }



    }
}
