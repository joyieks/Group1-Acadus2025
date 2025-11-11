namespace ASI.Basecode.WebApp.Interfaces
{
    /// <summary>
    /// Interface for StudentProfile model.
    /// </summary>
    public interface IStudentProfile
    {
        int id { get; set; }
        int yearLevel { get; set; }
        int programId { get; set; }
        int departmentId { get; set; }
    }
}
