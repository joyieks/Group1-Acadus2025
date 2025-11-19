using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface ITeacherCourseActivityRepository
    {
        Task<List<ActivityModel>> GetActivitiesByCourseAsync(int courseId);
        Task<ActivityModel> GetActivityByIdAsync(int activityId);
        Task CreateActivityAsync(ActivityModel activity);
        Task UpdateActivityAsync(ActivityModel activity);
        Task DeleteActivityAsync(int activityId);
        
        Task<List<SupabaseUserNew>> GetStudentsByCourseIdAsync(int courseId);
        Task<List<ActivitySubmissionModel>> GetSubmissionsByCourseAsync(int courseId);
        Task<ActivitySubmissionModel> GetSubmissionAsync(int activityId, string studentId);
        Task SaveSubmissionAsync(ActivitySubmissionModel submission);
    }
}

