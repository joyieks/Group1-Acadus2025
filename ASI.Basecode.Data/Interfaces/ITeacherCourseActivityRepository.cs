using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface ITeacherCourseActivityRepository
    {
        // Activities
        Task<List<ActivityModel>> GetActivitiesByCourseAsync(int courseId);
        Task<ActivityModel> GetActivityByIdAsync(int activityId);
        Task CreateActivityAsync(ActivityModel activity);
        Task UpdateActivityAsync(ActivityModel activity);

        // Students - return user records from users table
        Task<List<SupabaseUserNew>> GetStudentsByCourseIdAsync(int courseId);

        // Submissions
        Task<List<ActivitySubmissionModel>> GetSubmissionsByCourseAsync(int courseId);
        Task SaveSubmissionAsync(ActivitySubmissionModel model);
    }
}
