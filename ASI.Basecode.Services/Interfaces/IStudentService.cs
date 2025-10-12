using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IStudentService
    {
        Task<bool> CreateStudentAsync(StudentViewModel model);
        Task<Student> GetStudentByIdAsync(int id);
        Task<Student> GetStudentByEmailAsync(string email);
        Task<bool> UpdateStudentAsync(StudentViewModel model);
        Task<bool> DeleteStudentAsync(int id);
    }
}
