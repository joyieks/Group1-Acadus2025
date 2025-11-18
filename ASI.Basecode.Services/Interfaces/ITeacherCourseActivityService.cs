using ASI.Basecode.Service.ServiceModels;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface ITeacherCourseActivityService
    {
        Task<TeacherCourseModel> LoadCourseActivityPageAsync(int courseId);

        Task<TeacherActivityModel> GetActivityDetailsAsync(int activityId);

        Task CreateActivityAsync(TeacherActivityModel model);

        Task UpdateActivityAsync(TeacherActivityModel model);

        Task GradeActivityAsync(TeacherActivitySubmissionModel model);
    }
}
