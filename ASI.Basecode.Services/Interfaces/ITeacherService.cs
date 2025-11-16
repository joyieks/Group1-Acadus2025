using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface ITeacherService
    {
        Task<bool> CreateTeacherAsync(TeacherViewModel model);
        Task<SupabaseUserNew> GetTeacherByIdAsync(int id);  // Changed return type
        Task<SupabaseUserNew> GetTeacherByEmailAsync(string email);  // Changed return type
        Task<bool> UpdateTeacherAsync(TeacherViewModel model);  // Changed parameter type
        Task<bool> DeleteTeacherAsync(int id);
    }
}
