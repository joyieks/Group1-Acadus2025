using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IAdminService
    {
        Task<(int TotalStudents, int TotalInstructors, int TotalCourses)> GetDashboardStatisticsAsync();
    }
}