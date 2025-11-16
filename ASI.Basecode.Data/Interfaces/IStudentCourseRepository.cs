using System.Collections.Generic;
using System.Threading.Tasks;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IStudentCourseRepository
    {
        // Change parameter to string
        Task<List<CourseModel>> GetCoursesByStudentIdAsync(string studentId);

        Task<List<ActivityModel>> GetActivitiesByCourseIdAsync(long courseId);
        Task<List<ActivitySubmissionModel>> GetSubmissionsByStudentAndCourseAsync(string studentId, long courseId);

        Task<SupabaseUserNew> GetUserByUserTypeIdAsync(string userTypeId);

        Task<List<StudentReportViewModel.ReportItem>> GetStudentCourseReportsAsync(string studentId);

        Task<StudentDashboardViewModel> GetStudentDashboardAsync(string studentId);

    }
}
