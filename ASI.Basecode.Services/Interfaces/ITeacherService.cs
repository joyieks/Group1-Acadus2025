using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface ITeacherService
    {
        Task<bool> CreateTeacherAsync(TeacherViewModel model);
        Task<SupabaseUserNew> GetTeacherByIdAsync(int id);  
        Task<SupabaseUserNew> GetTeacherByEmailAsync(string email);  
        Task<bool> UpdateTeacherAsync(TeacherViewModel model);  
       
    }
}
