using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IStudentService
    {
        Task<bool> CreateStudentAsync(StudentViewModel model);
        Task<SupabaseUserNew> GetStudentByIdAsync(int id);  // Changed return type
        Task<SupabaseUserNew> GetStudentByEmailAsync(string email);  // Changed return type
        Task<SupabaseUserNew> GetStudentBySupabaseIdAsync(string supabaseUserId);
        Task<bool> UpdateStudentAsync(StudentViewModel model);
        Task<bool> DeleteStudentAsync(int id);
    }
}
