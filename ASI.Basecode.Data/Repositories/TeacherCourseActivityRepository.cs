using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Supabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Supabase.Postgrest.Constants;

namespace ASI.Basecode.Data.Repositories
{
    public class TeacherCourseActivityRepository : ITeacherCourseActivityRepository
    {
        private readonly Client _client;

        public TeacherCourseActivityRepository()
        {
            _client = AsiBasecodeDBContext.SupabaseClient;
        }

        // -------------------- ACTIVITIES --------------------
        public async Task<List<ActivityModel>> GetActivitiesByCourseAsync(int courseId)
        {
            var response = await _client
                .From<ActivityModel>()
                .Where(a => a.CourseId == courseId)
                .Get();

            return response.Models;
        }

        public async Task<ActivityModel> GetActivityByIdAsync(int activityId)
        {
            var activity = await _client
                .From<ActivityModel>()
                .Where(a => a.Id == activityId)
                .Single();

            return activity;
        }

        public async Task CreateActivityAsync(ActivityModel activity)
        {
            await _client.From<ActivityModel>().Insert(activity);
        }

        public async Task UpdateActivityAsync(ActivityModel activity)
        {
            await _client.From<ActivityModel>().Update(activity);
        }


        public async Task<List<SupabaseUserNew>> GetStudentsByCourseIdAsync(int courseId)
        {
            // 1) Get enrollments
            var enrollmentsRes = await _client
                .From<EnrollmentModel>()
                .Where(e => e.CourseId == courseId && e.Status == "Active")
                .Get();

            var studentIds = enrollmentsRes.Models?
                .Select(e => e.StudentId)   // string UUIDs
                .ToArray()
                ?? Array.Empty<string>();

            if (!studentIds.Any())
                return new List<SupabaseUserNew>();

            // 2) Match studentId (enrollment) -> userTypeId (users)
            var usersRes = await _client
                .From<SupabaseUserNew>()
                .Filter("userTypeId", Operator.In, studentIds.Select(s => (object)s).ToList())
                .Get();

            return usersRes.Models?.ToList() ?? new List<SupabaseUserNew>();
        }


        public async Task<List<ActivitySubmissionModel>> GetSubmissionsByCourseAsync(int courseId)
        {
            // 1. Get all activities for this course
            var activities = await GetActivitiesByCourseAsync(courseId);
            var activityIds = activities.Select(a => a.Id).ToList();

            if (!activityIds.Any())
                return new List<ActivitySubmissionModel>();

            // 2. Get submissions that match ANY activityId
            var response = await _client
                .From<ActivitySubmissionModel>()
                .Filter("activityId", Operator.In, activityIds.Cast<object>().ToList())
                .Get();

            return response.Models;
        }

        // Retrieves submissions by activity and student
        public async Task<ActivitySubmissionModel> GetSubmissionAsync(int activityId, string studentId)
        {
            var res = await _client
                .From<ActivitySubmissionModel>()
                .Where(s => s.ActivityId == activityId && s.StudentId == studentId)
                .Get();

            return res.Models.FirstOrDefault();
        }

        // Activity Grades Submission
        public async Task SaveSubmissionAsync(ActivitySubmissionModel model)
        {
            // Check if submission already exists
            var existing = await GetSubmissionAsync(model.ActivityId, model.StudentId);

            if (existing == null)
            {
                // NEW SUBMISSION → INSERT
                model.Id = null; // Supabase will generate UUID
                model.CreatedAt = DateTime.UtcNow;

                await _client
                    .From<ActivitySubmissionModel>()
                    .Insert(model);
            }
            else
            {
                // EXISTING SUBMISSION → UPDATE
                model.Id = existing.Id;         // <-- VERY IMPORTANT
                model.CreatedAt = existing.CreatedAt; // preserve original timestamp
                model.SubmissionStatus = model.SubmissionStatus ?? existing.SubmissionStatus;

                await _client
                    .From<ActivitySubmissionModel>()
                    .Update(model);
            }
        }


    }
}
