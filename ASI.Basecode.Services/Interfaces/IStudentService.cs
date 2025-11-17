using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IStudentService
    {
        Task<bool> CreateStudentAsync(StudentViewModel model);
        Task<SupabaseUserNew> GetStudentByIdAsync(int id); 
        Task<SupabaseUserNew> GetStudentByEmailAsync(string email);  
        Task<SupabaseUserNew> GetStudentBySupabaseIdAsync(string supabaseUserId);
        Task<bool> UpdateStudentAsync(StudentViewModel model);
        
    }
}
