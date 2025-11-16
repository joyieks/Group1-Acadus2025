using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendStudentWelcomeEmailAsync(string email, string firstName, string lastName, string temporaryPassword);
        Task<bool> SendPasswordResetEmailAsync(string email, string firstName, string resetLink);
        Task<bool> SendStudentCredentialsEmailAsync(string email, string firstName, string lastName, string temporaryPassword);
    }
}
