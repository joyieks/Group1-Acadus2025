using ASI.Basecode.WebApp.Models;

namespace ASI.Basecode.WebApp.Interfaces
{
    /// <summary>
    /// Interface for User model repository operations.
    /// </summary>
    public interface IUser
    {
        int id { get; set; }
        string firstName { get; set; }
        string lastName { get; set; }
        string middleName { get; set; }
        string suffix { get; set; }
        string email { get; set; }
        string contactNumber { get; set; }
        int address { get; set; }
        int emergencyContact { get; set; }
        int userTypeId { get; set; }
        bool isActive { get; set; }
        string profilePictureUrl { get; set; }
    }
}
