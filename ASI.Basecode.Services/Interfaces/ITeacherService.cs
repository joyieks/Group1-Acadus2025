using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface ITeacherService
    {
        Task<bool> CreateTeacherAsync(TeacherViewModel model);
        Task<Teacher> GetTeacherByIdAsync(int id);
        Task<Teacher> GetTeacherByEmailAsync(string email);
        Task<bool> UpdateTeacherAsync(Teacher teacher);
        Task<bool> DeleteTeacherAsync(int id);
    }
}
