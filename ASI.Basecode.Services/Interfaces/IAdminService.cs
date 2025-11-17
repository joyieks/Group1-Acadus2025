using System.Collections.Generic;
using System.Threading.Tasks;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IAdminService
    {
        Task<(int TotalStudents, int TotalInstructors, int TotalCourses)> GetDashboardStatisticsAsync();
  
        /// <summary>
        /// Gets all programs from the database
        /// </summary>
        Task<List<ASI.Basecode.Data.Models.Program>> GetAllProgramsAsync();
        
        /// <summary>
        /// Gets all departments from the database
        /// </summary>
        Task<List<Department>> GetAllDepartmentsAsync();
    }
}