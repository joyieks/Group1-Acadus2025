using ASI.Basecode.Services.ServiceModels;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface ITeacherCourseActivityService
    {
        Task<TeacherCourseModel> LoadCourseActivityPageAsync(int courseId);
        Task<TeacherActivityModel> GetActivityDetailsAsync(int activityId);
        Task CreateActivityAsync(TeacherActivityModel model);
        Task UpdateActivityAsync(TeacherActivityModel model);
        Task DeleteActivityAsync(int activityId);
        Task GradeActivityAsync(TeacherActivitySubmissionModel submission);
    }
}
