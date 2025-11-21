using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Supabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ASI.Basecode.Data.Models.CourseGradebookViewModel;
using static Supabase.Postgrest.Constants;

namespace ASI.Basecode.Data.Repositories
{
    public class TeacherCourseRepository : ITeacherCourseRepository
    {
        private readonly Client _supabaseClient;

        public TeacherCourseRepository(Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<CourseModel> GetCourseAsync(int courseId)
        {
            return (await _supabaseClient
                .From<CourseModel>()
                .Where(c => c.Id == courseId)
                .Get()).Models.FirstOrDefault();
        }

        public async Task<List<EnrollmentModel>> GetActiveEnrollmentsAsync(int courseId)
        {
            return (await _supabaseClient
                .From<EnrollmentModel>()
                .Where(e => e.CourseId == courseId && e.Status == "Active")
                .Get()).Models;
        }

        public async Task<List<ActivityModel>> GetActivitiesByCourseIdAsync(long courseId)
        {
            var res = await _supabaseClient
                .From<ActivityModel>()
                .Filter("courseId", Operator.Equals, courseId)
                .Filter("isVisible", Operator.Equals, "true")
                .Get();

            return res.Models.OrderBy(a => a.DueDate).ToList();
        }

        public async Task<List<ActivitySubmissionModel>> GetSubmissionsByStudentAndCourseAsync(string studentId, long courseId)
        {
            var activities = await GetActivitiesByCourseIdAsync(courseId);
            var activityIds = activities.Select(a => a.Id).ToList();

            if (!activityIds.Any()) return new List<ActivitySubmissionModel>();

            var res = await _supabaseClient
                .From<ActivitySubmissionModel>()
                .Where(s => s.StudentId == studentId)
                .Filter("activityId", Operator.In, activityIds.ToArray())
                .Get();

            return res.Models;
        }

        public async Task<SupabaseUserNew> GetUserByUserTypeIdAsync(string userTypeId)
        {
            return (await _supabaseClient
                .From<SupabaseUserNew>()
                .Where(u => u.UserTypeId == userTypeId)
                .Get()).Models.FirstOrDefault();
        }

        public async Task<ActivitySubmissionModel?> GetSubmissionAsync(string studentId, int activityId)
        {
            return (await _supabaseClient
                .From<ActivitySubmissionModel>()
                .Where(s => s.StudentId == studentId)
                .Where(s => s.ActivityId == activityId)
                .Get()).Models.FirstOrDefault();
        }

        public async Task<bool> UpdateSubmissionAsync(ActivitySubmissionModel submission)
        {
            await submission.Update<ActivitySubmissionModel>();
            return true;
        }
    }

}
