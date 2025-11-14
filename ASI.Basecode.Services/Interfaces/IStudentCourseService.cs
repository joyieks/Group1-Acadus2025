using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IStudentCourseService
    {
        Task<List<CourseModel>> GetCoursesByStudentAsync(string studentId);

        Task<StudentCourseDetailsViewModel> GetCourseDetailsAsync(string studentId, string courseId);

        Task<List<StudentReportViewModel.ReportItem>> GetStudentReportsAsync(string studentId);
    }
}
